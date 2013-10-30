using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sqor.Utils.Generics;
using Sqor.Utils.Strings;
using Sqor.Utils.Json;
using System.Globalization;

namespace Sqor.Utils.Json
{
	public class JsonObjectSerializer : IJsonSerializer
	{
//        private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal const string dateFormat = @"yyyy-MM-dd\THH:mm:ss\Z";

		public JsonValue Parse(string input)
		{
			return JsonValue.Parse(input);
		}		

		public T Parse<T>(string input)
		{
			var jsonValue = JsonValue.Parse(input);
			return (T)ConvertJsonObjectToType(jsonValue, typeof(T));
		}

		public string Serialize(object o)
		{
            using (var s = new StringWriter()) 
            {
                ConvertObjectToJsonValue(o).Save(s);
                return s.ToString();
            }
		}
        
        public static DateTime? ParseDate(string s)
        {
            if (s == "0000-00-00T00:00:00Z")
                return null;
            if (s.EndsWith("Z"))
                try
                {
                    return DateTime.SpecifyKind(DateTime.ParseExact(s, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal), DateTimeKind.Utc);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Invalid DateTime: " + s, e);
                }
            else
                return DateTime.ParseExact(s, dateFormat, null);
        }

		static internal object ConvertJsonObjectToType(JsonValue graph, Type type) 
		{
            // Custom converter?
            var converterAttribute = type.GetCustomAttribute<JsonConverterAttribute>();
            if (converterAttribute != null)
            {
                var converter = (IJsonConverter)Activator.CreateInstance(converterAttribute.Converter);
                return converter.FromJson(graph);
            }

            if (type.IsNullableValueType())
                type = type.GetNullableValueType();

			if (type == typeof(string)) 
			{
				return (string)graph;
			}
            else if (type.IsEnum)
            {
                var enumCache = EnumCache.GetInstance(type);
                if (graph == null)
                    return enumCache.NullValue;
                else
                    return enumCache.EnumsByKey[graph];
            }
			else if (type == typeof(bool)) 
			{
				return (bool)graph;
			}
			else if (type == typeof(int))
			{
				return (int)graph;
			}
			else if (type == typeof(long)) 
			{
				return (long)graph;
			}
			else if (type == typeof(float)) 
			{
				return (float)graph;
			}
			else if (type == typeof(double))
			{
				return (double)graph;
			}
            else if (type == typeof(decimal)) 
            {
                return (decimal)graph;
            }
            else if (type == typeof(DateTime))
            {
                var s = (string)graph;
                if (s == null)
                    return null;
                return ParseDate(s);
            }
            else if (type.IsGenericDictionary())
            {
                var dictionary = (IDictionary)Activator.CreateInstance(type);
                var jsonObject = (JsonObject)graph;
                foreach (var item in jsonObject)
                {
                    dictionary[item.Key] = item.Value;
                }
                return dictionary;
            }
			else if (type.IsArray)
			{
                if (graph is JsonArray)
                {
                    var array = (JsonArray)graph;
                    var result = Array.CreateInstance(type.GetElementType(), array.Count);
                    for (int i = 0; i < result.Length; i++)
                    {
                        var jsonValue = array[i];
                        result.SetValue(ConvertJsonObjectToType(jsonValue, type.GetElementType()), i);
                    }
                    return result;
                }
                else if (graph is JsonPrimitive && ((JsonPrimitive)graph).Value == null)
                {
                    return null;
                }
                else
                {
                    throw new InvalidOperationException("Expected an array or the null value, but found: " + graph.ToString()); // ToString is not the same as implicit conversion since we have user type conversions -- ignore resharper
                }
			}
			else if (type.IsGenericList())
			{
				var array = (JsonArray)graph;
				var elementType = type.GetListElementType();
				var listType = typeof(List<>).MakeGenericType(elementType);
				var list = (IList)Activator.CreateInstance(listType);
				for (int i = 0; i < array.Count; i++)
				{
					var jsonValue = array[i];
					list.Add(ConvertJsonObjectToType(jsonValue, elementType));
				}
				return list;
			}
			else 
			{
				var jsonObject = (JsonObject)graph;
                return jsonObject.To(type);
			}
		}

		static internal JsonValue ConvertObjectToJsonValue(object graph) 
		{
            if (graph == null)
            {
                return new JsonPrimitive();
            }
            
			var type = graph.GetType();

            // Custom converter?
            var converterAttribute = type.GetCustomAttribute<JsonConverterAttribute>();
            if (converterAttribute != null)
            {
                var converter = (IJsonConverter)Activator.CreateInstance(converterAttribute.Converter);
                return converter.ToJson(graph);
            }

            if (graph is JsonValue)
            {
                return (JsonValue)graph;
            }
			else if (graph is string)
			{
				return (string)graph;
			}
            else if (graph is Enum)
            {
                var enumCache = EnumCache.GetInstance(graph.GetType());
                if (graph.Equals(enumCache.NullValue))
                    return new JsonPrimitive();
                else
                    return JsonAttribute.GetKey((Enum)graph);
            }
			else if (type == typeof(bool)) 
			{
				return (bool)graph;
			}
			else if (type == typeof(int)) 
			{
				return (int)graph;
			}
			else if (type == typeof(long)) 
			{
				return (long)graph;
			}
			else if (type == typeof(float)) 
			{
				return (float)graph;
			}
			else if (type == typeof(double)) 
			{
				return (double)graph;
			}
            else if (type == typeof(decimal))
            {
                return (decimal)graph;
            }
            else if (type == typeof(DateTime))
            {
                var date = (DateTime)graph;
                var s = date.ToString(dateFormat);
                return s;
            }
			else if (type.IsArray) 
			{
				var array = (Array)graph;
				var elements = new List<JsonValue>();

				for (int i = 0; i < array.Length; i++)
				{
					var value = array.GetValue(i);
					var jsonValue = ConvertObjectToJsonValue(value);
					elements.Add(jsonValue);
				}

				var result = new JsonArray(elements);
				return result;
			}
			else if (type.IsGenericList())
			{
				var list = (IList)graph;
				var elements = new List<JsonValue>();
				
				foreach (var value in list)
				{
				    var jsonValue = ConvertObjectToJsonValue(value);
				    elements.Add(jsonValue);
				}
				
				var result = new JsonArray(elements);
				return result;
			}
            else if (graph is IDictionary)
            {
                var dict = (IDictionary)graph;
                var values = new List<KeyValuePair<string, JsonValue>>();
                
                foreach (string key in dict.Keys)
                {
                    try 
                    {
                        var value = dict[key];
                        var jsonValue = ConvertObjectToJsonValue(value);
                        values.Add(new KeyValuePair<string, JsonValue>(key, jsonValue));
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException("Error serializing property " + key, e);
                    }
                }
                
                var result = new JsonObject(values);
                return result;
            }
			else 
			{
                return JsonObject.From(graph);
			}
		}
	}
}
