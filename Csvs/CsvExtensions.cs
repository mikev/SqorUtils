using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sqor.Utils.Csvs
{
    public static class CsvExtensions
    {
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

        public static IEnumerable<CsvRow> ParseCsv(this string input)
        {
            using (var reader = new StringReader(input))
            {
                foreach (var row in reader.ParseCsv())
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