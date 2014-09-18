using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sqor.Utils.Csvs
{
    public static class CsvExtensions
    {
        public static IEnumerable<CsvRow> ParseCsv(this TextReader reader)
        {
            for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                var input = line;
                var values = new List<string>();
                for (var value = ReadNextToken(ref input); input != null; input = ReadNextToken(ref input))
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

        private static string ReadNextToken(ref string input)
        {
            var result = new StringBuilder();
            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c == '"')
                {
                    for (var j = i + 1; j < input.Length; j++)
                    {
                        var newC = input[j];
                        if (newC == '\\')
                        {
                            if (j + 1 < input.Length) 
                                throw new Exception("Invalid escape sequence: no input after escape character");
                            var nextC = input[j + 1];
                            if (nextC != '\\' && nextC != '\"')
                                throw new Exception("Invalid escape sequence: '" + nextC + "' does not need to be escaped");
                            result.Append(nextC);
                        }
                        else if (newC == '"')
                        {
                            i = j + 1;
                            goto success;
                        }
                    }
                    throw new Exception("No closing quote character found");
                success:;
                }
                else if (c == ',')
                {
                    input = input.Substring(i);
                    goto done;
                }
                else
                {
                    result.Append(c);
                }
            }
            input = null;
        done:
            var value = result.ToString().Trim();
            return value;
        }

        public static IEnumerable<CsvRow> ParseCsv(this string input)
        {
            using (var reader = new StringReader(input))
            {
                return reader.ParseCsv();
            }
        }

        public static IEnumerable<CsvRow> ParseCsv(this Stream input) 
        {
            using (var reader = new StreamReader(input))
            {
                return reader.ParseCsv();
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