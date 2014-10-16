using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sqor.Utils.Enumerables;

namespace Sqor.Utils.Csvs
{
    public static class CsvExtensions
    {
        public class CsvColumn<T, TValue> : ICsvColumn<T>
        {
            public string Name { get; set; }
            public Func<T, TValue> Getter { get; set; }

            Func<T, object> ICsvColumn<T>.Getter
            {
                get { return x => Getter(x); }
            }
        }

        public interface ICsvColumn<in T>
        {
            string Name { get; }
            Func<T, object> Getter { get; }
        }

        public class ColumnCreator<T>
        {
            internal List<ICsvColumn<T>> columns = new List<ICsvColumn<T>>();

            public void Column<TValue>(string name, Func<T, TValue> getter)
            {
                columns.Add(new CsvColumn<T, TValue> { Name = name, Getter = getter });
            }
        }

        public static string WriteCsv<T>(this IEnumerable<T> rows, Action<ColumnCreator<T>> columns)
        {
            var creator = new ColumnCreator<T>();
            columns(creator);

            var builder = new StringBuilder();
            foreach (var column in creator.columns.SelectPosition())
            {
                builder.Append(column.Item.Name);
                if (!column.IsLast)
                    builder.Append(",");
            }
            builder.AppendLine();

            foreach (var row in rows)
            {
                foreach (var column in creator.columns.SelectPosition())
                {
                    var value = column.Item.Getter(row);
                    if (value is string && ((string)value).Contains(","))
                        value = "\"" + ((string)value).Replace("\"", "\"\"") + "\"";
                    if (value != null)
                        builder.Append(value);
                    if (!column.IsLast)
                        builder.Append(",");
                }
                builder.AppendLine();
            }

            return builder.ToString();
        }

        public static IEnumerable<CsvRow> ParseCsv(this TextReader reader, bool hasHeaderRow = false)
        {
            var skippedHeaderRow = !hasHeaderRow;
            for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                if (!skippedHeaderRow)
                {
                    skippedHeaderRow = true;
                    continue;
                }
                var input = line;
                var values = new List<string>();
                for (string value; ReadNextToken(ref input, out value);)
                {
                    values.Add(value);
                }
                var row = new CsvRow(values.Count);
                for (var i = 0; i < values.Count; i++)
                {
                    row[i] = values[i];
                }
                yield return row;
            }
        }

        private static bool ReadNextToken(ref string input, out string value)
        {
            if (input == null)
            {
                value = null;
                return false;
            }

            var result = new StringBuilder();
            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c == '"')
                {
                    for (var j = i + 1; j < input.Length; j++)
                    {
                        var newC = input[j];
                        if (newC == '\"')
                        {
                            if (j < input.Length - 1)
                            {
                                var nextC = input[j + 1];
                                if (nextC == '"')
                                {
                                    result.Append(newC);
                                    j++;
                                    continue;
                                }
                            }
                            i = j;
                            goto success;
                        }
                        else
                        {
                            result.Append(newC);
                        }
                    }
                    throw new Exception("No closing quote character found");
                success:;
                }
                else if (c == ',')
                {
                    input = input.Substring(i + 1);
                    goto done;
                }
                else
                {
                    result.Append(c);
                }
            }
            input = null;
        done:
            value = result.ToString().Trim();
            return true;
        }

        public static IEnumerable<CsvRow> ParseCsv(this string input, bool hasHeaderRow = false)
        {
            using (var reader = new StringReader(input))
            {
                foreach (var row in reader.ParseCsv(hasHeaderRow))
                    yield return row;
            }
        }

        public static IEnumerable<CsvRow> ParseCsv(this Stream input, bool hasHeaderRow = false) 
        {
            using (var reader = new StreamReader(input))
            {
                foreach (var row in reader.ParseCsv(hasHeaderRow))
                    yield return row;
            }
        }

    }

    public class CsvRow
    {
        private string[] values;

        public CsvRow(int numberOfColumns)
        {
            values = new string[numberOfColumns];
        }

        public string this[int index]
        {
            get { return values[index]; }
            set { values[index] = value; }
        }

        public int Count
        {
            get { return values.Length; }
        }
    }
}