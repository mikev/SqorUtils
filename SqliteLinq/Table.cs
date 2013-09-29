using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Data.Common;
using System.Linq;
using System.Collections;
using System.Reflection;
using Sqor.Utils.Logging;
using Sqor.Utils.Generics;

namespace Sqor.Utils.SqliteLinq
{
    public class Table<T> : IEnumerable<T> where T : new()
    {
        private DataContext dataContext;
        private Mapping mapping;
        private Expression @where;
        private List<Ordering> orderBys;
        private int? limit;
        private int? offset;

        public Table(DataContext dataContext, Mapping mapping)
        {
            this.dataContext = dataContext;
            this.mapping = mapping;
        }
            
        class Ordering
        {
            public string ColumnName { get; set; }
            public bool Ascending { get; set; }
        }
        
        public Table<T> Clone()
        {
            var q = new Table<T>(dataContext, mapping);
            q.where = @where;
            if (orderBys != null) 
            {
                q.orderBys = new List<Ordering>(orderBys);
            }
            q.limit = limit;
            q.offset = offset;
            return q;
        }
        
        public Table<T> Where(Expression<Func<T, bool>> predExpr)
        {
            if (predExpr.NodeType == ExpressionType.Lambda) 
            {
                var lambda = (LambdaExpression)predExpr;
                var pred = lambda.Body;
                var q = Clone();
                q.AddWhere(pred);
                return q;
            } 
            else 
            {
                throw new NotSupportedException("Must be a predicate");
            }
        }
        
        public Table<T> Take(int n)
        {
            var q = Clone();
            q.limit = n;
            return q;
        }
        
        public Table<T> Skip(int n)
        {
            var q = Clone();
            q.offset = n;
            return q;
        }
        
        public T ElementAt(int index)
        {
            return Skip(index).Take(1).First();
        }
        
        public Table<T> OrderBy<U> (Expression<Func<T, U>> orderExpr)
        {
            return AddOrderBy<U>(orderExpr, true);
        }
        
        public Table<T> OrderByDescending<U> (Expression<Func<T, U>> orderExpr)
        {
            return AddOrderBy<U>(orderExpr, false);
        }
        
        private Table<T> AddOrderBy<U> (Expression<Func<T, U>> orderExpr, bool asc)
        {
            if (orderExpr.NodeType == ExpressionType.Lambda) 
            {
                var lambda = (LambdaExpression)orderExpr;
                var mem = lambda.Body as MemberExpression;
                if (mem != null && (mem.Expression.NodeType == ExpressionType.Parameter)) 
                {
                    var q = Clone();
                    if (q.orderBys == null) 
                    {
                        q.orderBys = new List<Ordering>();
                    }
                    q.orderBys.Add(new Ordering 
                    {
                        ColumnName = mapping.GetColumnName(mem.Member.Name),
                        Ascending = asc
                    });
                    return q;
                } 
                else 
                {
                    throw new NotSupportedException ("Order By does not support: " + orderExpr);
                }
            } 
            else 
            {
                throw new NotSupportedException ("Must be a predicate");
            }
        }
        
        private void AddWhere (Expression pred)
        {
            if (where == null) 
            {
                where = pred;
            } 
            else 
            {
                where = Expression.AndAlso(where, pred);
            }
        }
        
        public Table<TResult> Join<TInner, TKey, TResult> (
            Table<TInner> inner,
            Expression<Func<T, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<T, TInner, TResult>> resultSelector
        )
            where TResult : new()
            where TInner : new()
        {
            throw new NotImplementedException();
        }
        
        private DbCommand GenerateCommand(string selectionList)
        {
            var cmdText = "select " + selectionList + " from \"" + mapping.TableName + "\"";
            var args = new List<object>();
            if (where != null) 
            {
                var w = CompileExpr(where, args);
                cmdText += " where " + w.CommandText;
            }
            if ((orderBys != null) && (orderBys.Count > 0)) 
            {
                var t = string.Join(", ", orderBys.Select(o => "\"" + o.ColumnName + "\"" + (o.Ascending ? "" : " desc")).ToArray());
                cmdText += " order by " + t;
            }
            if (limit.HasValue) 
            {
                cmdText += " limit " + limit.Value;
            }
            if (offset.HasValue) 
            {
                if (!limit.HasValue) 
                {
                    cmdText += " limit -1 ";
                }
                cmdText += " offset " + offset.Value;
            }
            var command = dataContext.Connection.CreateCommand();
            command.CommandText = cmdText;
            foreach (var arg in args.Select((x, i) => new { Index = i + 1, Value = x }))
            {
                var parameter = command.CreateParameter();
                parameter.Value = arg.Value;
//                parameter.ParameterName = arg.Index.ToString();
                command.Parameters.Add(parameter);
            }
            return command;
        }
        
        class CompileResult
        {
            public string CommandText { get; set; }
            
            public object Value { get; set; }
        }
        
        private CompileResult CompileExpr(Expression expr, List<object> queryArgs)
        {
            if (expr == null) 
            {
                throw new NotSupportedException("Expression is NULL");
            } 
            else if (expr is BinaryExpression) 
            {
                var bin = (BinaryExpression)expr;
                
                var leftr = CompileExpr(bin.Left, queryArgs);
                var rightr = CompileExpr(bin.Right, queryArgs);
                
                //If either side is a parameter and is null, then handle the other side specially (for "is null"/"is not null")
                string text;
                if (leftr.CommandText == "?" && leftr.Value == null)
                    text = CompileNullBinaryExpression(bin, rightr);
                else if (rightr.CommandText == "?" && rightr.Value == null)
                    text = CompileNullBinaryExpression(bin, leftr);
                else
                    text = "(" + leftr.CommandText + " " + GetSqlName(bin) + " " + rightr.CommandText + ")";
                return new CompileResult { CommandText = text };
            } 
            else if (expr.NodeType == ExpressionType.Call) 
            {
                var call = (MethodCallExpression)expr;
                var args = new CompileResult[call.Arguments.Count];
                var obj = call.Object != null ? CompileExpr (call.Object, queryArgs) : null;
                
                for (var i = 0; i < args.Length; i++) 
                {
                    args [i] = CompileExpr (call.Arguments [i], queryArgs);
                }
                
                var sqlCall = "";
                
                if (call.Method.Name == "Like" && args.Length == 2) 
                {
                    sqlCall = "(" + args [0].CommandText + " like " + args [1].CommandText + ")";
                }
                else if (call.Method.Name == "Contains" && args.Length == 2) 
                {
                    sqlCall = "(" + args [1].CommandText + " in " + args [0].CommandText + ")";
                }
                else if (call.Method.Name == "Contains" && args.Length == 1) 
                {
                    if (call.Object != null && call.Object.Type == typeof(string)) 
                    {
                        sqlCall = "(" + obj.CommandText + " like ('%' || " + args [0].CommandText + " || '%'))";
                    }
                    else 
                    {
                        sqlCall = "(" + args [0].CommandText + " in " + obj.CommandText + ")";
                    }
                }
                else if (call.Method.Name == "StartsWith" && args.Length == 1) 
                {
                    sqlCall = "(" + obj.CommandText + " like (" + args [0].CommandText + " || '%'))";
                }
                else if (call.Method.Name == "EndsWith" && args.Length == 1) 
                {
                    sqlCall = "(" + obj.CommandText + " like ('%' || " + args [0].CommandText + "))";
                }
                else 
                {
                    sqlCall = call.Method.Name.ToLower () + "(" + string.Join (",", args.Select (a => a.CommandText).ToArray ()) + ")";
                }
                return new CompileResult { CommandText = sqlCall };
            } 
            else if (expr.NodeType == ExpressionType.Constant) 
            {
                var c = (ConstantExpression)expr;
                queryArgs.Add(c.Value);
                return new CompileResult 
                {
                    CommandText = "?",
                    Value = c.Value
                };
            } 
            else if (expr.NodeType == ExpressionType.Convert) 
            {
                var u = (UnaryExpression)expr;
                var ty = u.Type;
                var valr = CompileExpr (u.Operand, queryArgs);
                return new CompileResult 
                {
                    CommandText = valr.CommandText,
                    Value = valr.Value != null ? Convert.ChangeType (valr.Value, ty, null) : null
                };
            } 
            else if (expr.NodeType == ExpressionType.MemberAccess) 
            {
                var mem = (MemberExpression)expr;
                
                if (mem.Expression.NodeType == ExpressionType.Parameter) 
                {
                    //
                    // This is a column of our table, output just the column name
                    // Need to translate it if that column name is mapped
                    //
                    var columnName = mapping.GetColumnName(mem.Member.Name);
                    return new CompileResult { CommandText = "\"" + columnName + "\"" };
                } 
                else 
                {
                    object obj = null;
                    if (mem.Expression != null) 
                    {
                        var r = CompileExpr (mem.Expression, queryArgs);
                        if (r.Value == null) 
                        {
                            throw new NotSupportedException ("Member access failed to compile expression");
                        }
                        if (r.CommandText == "?") 
                        {
                            queryArgs.RemoveAt (queryArgs.Count - 1);
                        }
                        obj = r.Value;
                    }
                    
                    //
                    // Get the member value
                    //
                    object val = null;
                    
                    if (mem.Member.MemberType == MemberTypes.Property) 
                    {
                        var m = (PropertyInfo)mem.Member;
                        val = m.GetValue (obj, null);
                    } 
                    else if (mem.Member.MemberType == MemberTypes.Field) 
                    {
                        var m = (FieldInfo)mem.Member;
                        val = m.GetValue(obj);
                    } 
                    else 
                    {
                        throw new NotSupportedException ("MemberExpr: " + mem.Member.MemberType.ToString ());
                    }
                        
                    //
                    // Work special magic for enumerables
                    //
                    if (val != null && val is System.Collections.IEnumerable && !(val is string)) 
                    {
                        var sb = new System.Text.StringBuilder();
                        sb.Append("(");
                        var head = "";
                        foreach (var a in (System.Collections.IEnumerable)val) 
                        {
                            queryArgs.Add(a);
                            sb.Append(head);
                            sb.Append("?");
                            head = ",";
                        }
                        sb.Append(")");
                        return new CompileResult 
                        {
                            CommandText = sb.ToString(),
                            Value = val
                        };
                    }
                    else 
                    {
                        queryArgs.Add (val);
                        return new CompileResult 
                        {
                            CommandText = "?",
                            Value = val
                        };
                    }
                }
            }
            throw new NotSupportedException ("Cannot compile: " + expr.NodeType.ToString ());
        }
            
        /// <summary>
        /// Compiles a BinaryExpression where one of the parameters is null.
        /// </summary>
        /// <param name="parameter">The non-null parameter</param>
        private string CompileNullBinaryExpression(BinaryExpression expression, CompileResult parameter)
        {
            if (expression.NodeType == ExpressionType.Equal)
                return "(" + parameter.CommandText + " is ?)";
            else if (expression.NodeType == ExpressionType.NotEqual)
                return "(" + parameter.CommandText + " is not ?)";
            else
                throw new NotSupportedException("Cannot compile Null-BinaryExpression with type " + expression.NodeType.ToString());
        }
        
        string GetSqlName(Expression expr)
        {
            var n = expr.NodeType;
            if (n == ExpressionType.GreaterThan) 
            {
                return ">"; 
            }
            else if (n == ExpressionType.GreaterThanOrEqual) 
            {
                return ">=";
            } 
            else if (n == ExpressionType.LessThan) 
            {
                return "<";
            } 
            else if (n == ExpressionType.LessThanOrEqual) 
            {
                return "<=";
            } 
            else if (n == ExpressionType.And) 
            {
                return "and";
            } 
            else if (n == ExpressionType.AndAlso) 
            {
                return "and";
            } 
            else if (n == ExpressionType.Or) 
            {
                return "or";
            } 
            else if (n == ExpressionType.OrElse) 
            {
                return "or";
            } 
            else if (n == ExpressionType.Equal) 
            {
                return "=";
            } 
            else if (n == ExpressionType.NotEqual) 
            {
                return "!=";
            } 
            else 
            {
                throw new System.NotSupportedException("Cannot get SQL for: " + n);
            }
        }
        
        public int Count()
        {
            return (int)GenerateCommand("count(*)").ExecuteScalar();           
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            using (var command = GenerateCommand("*"))
            {
                Trace(command.CommandText);            
                using (var reader = command.ExecuteReader())
                {
                    var columnNames = new string[reader.FieldCount];
                    for (var i = 0; i < columnNames.Length; i++)
                    {
                        columnNames[i] = reader.GetName(i);
                    }
    
                    while (reader.Read())
                    {
                        var obj = new T();
                        foreach (var columnName in columnNames)
                        {
                            var property = mapping.GetProperty(columnName);
                            object value = reader[columnName];
                            value = ConvertTo(value, property.PropertyType);
                            property.SetValue(obj, value, null);
                        }
                        yield return obj;
                    }
                }
            }
        }
        
        private object ConvertTo(object value, Type propertyType)
        {
            if (value is DBNull)
                return null;
            else if (propertyType.IsEnum)
                return Enum.Parse(propertyType, (string)value);
            else if (propertyType == typeof(TimeSpan) || propertyType == typeof(TimeSpan?))
                return TimeSpan.Parse((string)value);
            else if (value is IConvertible && value.GetType() != propertyType && propertyType.GetNullableValueType() != value.GetType())
                return Convert.ChangeType(value, propertyType);
            else
                return value;
        }
        
        private object ConvertFrom(object value, Type propertyType)
        {
            if (value == null)
                return null;
            else if (value is Enum)
                return value.ToString();
            else if (value is TimeSpan)
                return ((TimeSpan)value).ToString();
            else if (value is IConvertible && value.GetType() != propertyType && propertyType.GetNullableValueType() != value.GetType())
                return Convert.ChangeType(value, propertyType);
            else
                return value;
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public T First()
        {
            var query = Take(1);
            return query.First();
        }
        
        public T FirstOrDefault()
        {
            var query = this.Take(1);
            return query.FirstOrDefault();
        }
        
        private bool isTraceEnabled = false;
        
        private void Trace(string sql)
        {
            if (isTraceEnabled)
            {
                this.LogInfo(sql);
            }
        }

        public void Insert(T entity)
        {
            using (var command = dataContext.Connection.CreateCommand())
            {
                if (mapping.InsertColumnNames.Count() == 0 && mapping.AutoIncrementProperty != null)
                {
                    command.CommandText = string.Format("INSERT INTO {0} DEFAULT VALUES;");
                }
                else 
                {
                    command.CommandText = string.Format(
                        "INSERT INTO {0} ({1}) VALUES ({2});",
                        mapping.TableName,
                        string.Join(", ", mapping.InsertColumnNames),
                        string.Join(", ", mapping.InsertProperties.Select(x => "@" + x.Name)));
                        
                    foreach (var property in mapping.InsertProperties)
                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = "@" + property.Name;
                        parameter.Value = ConvertFrom(property.GetValue(entity, null), property.PropertyType);
                        command.Parameters.Add(parameter);
                    }
                }
                
                if (mapping.AutoIncrementProperty != null)
                {
                    command.CommandText += " SELECT last_insert_rowid();";
                    Trace(command.CommandText);
                    var id = command.ExecuteScalar();
                    id = ConvertTo(id, mapping.AutoIncrementProperty.PropertyType);
                    mapping.AutoIncrementProperty.SetValue(entity, id, null);
                }
                else
                {
                    Trace(command.CommandText);
                    command.ExecuteNonQuery();
                }
            }
        }

        public int Update(T entity)
        {
            using (var command = dataContext.Connection.CreateCommand())
            {
                command.CommandText = string.Format("UPDATE {0} SET {1} WHERE {2}",
                    mapping.TableName,
                    string.Join(", ", mapping.UpdateColumnNames.Select(x => x + " = @" + mapping.GetPropertyName(x))),
                    string.Join(" and ", mapping.PrimaryKeyProperties.Select(x => mapping.GetColumnName(x.Name) + " = @" + x.Name)));
                    
                foreach (var property in mapping.UpdateProperties.Union(mapping.PrimaryKeyProperties))
                {
                    var parameter = command.CreateParameter();
                    parameter.Value = ConvertFrom(property.GetValue(entity, null), property.PropertyType);
                    parameter.ParameterName = "@" + property.Name;
                    command.Parameters.Add(parameter);
                }
                
                Trace(command.CommandText);
                return command.ExecuteNonQuery();
            }
        }

        public bool Delete(T entity)
        {
            using (var command = dataContext.Connection.CreateCommand())
            {
                command.CommandText = string.Format("DELETE FROM {0} WHERE {1}",
                    mapping.TableName,
                    string.Join(" and ", mapping.PrimaryKeyProperties.Select(x => mapping.GetColumnName(x.Name) + " = @" + x.Name)));
                    
                foreach (var property in mapping.PrimaryKeyProperties)
                {
                    var parameter = command.CreateParameter();
                    parameter.Value = property.GetValue(entity, null);
                    parameter.ParameterName = "@" + property.Name;
                    command.Parameters.Add(parameter);
                }
                
                Trace(command.CommandText);                
                return command.ExecuteNonQuery() > 0;
            }
        }

        public bool TableExists()
        {
            return dataContext.Connection.GetSchema("TABLES", new[] { null, null, mapping.TableName }).Rows.Count > 0;
        }
    }
}