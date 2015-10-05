using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Sqor.Utils.Objects;
using System.Text.RegularExpressions;

namespace Sqor.Utils.Strings
{
    public static class StringExtensions
    {
        public static IEnumerable<string> AsCodePoints(this string s)
        {
            for (int i = 0; i < s.Length; ++i)
            {
                yield return char.ConvertFromUtf32(char.ConvertToUtf32(s, i));
                if (char.IsHighSurrogate(s, i))
                    i++;
            }
        }

        public static string FirstWord(this string s)
        {
            return s.FirstWord(" ");
        }

        public static string FirstWord(this string s, string delimiter)
        {
            return s.Copy(0, s.IndexOfElse(delimiter, delimiter.Length));
        }

        public static string NotFirstWord(this string s)
        {
            return s.NotFirstWord(" ");
        }

        public static string NotFirstWord(this string s, string delimiter)
        {
            return s.Copy(s.IndexOfElse(delimiter, -delimiter.Length) + delimiter.Length, s.Length);
        }

        public static string LastWord(this string s)
        {
            return s.LastWord(" ");
        }

        public static string LastWord(this string s, string delimiter)
        {
            return s.Copy(s.LastIndexOfElse(delimiter, -delimiter.Length) + delimiter.Length);
        }

        public static string NotLastWord(this string s)
        {
            return s.NotLastWord(" ");
        }

        public static string NotLastWord(this string s, string delimiter)
        {
            return s.Copy(0, s.LastIndexOfElse(delimiter, 0));
        }

        public static int IndexOfElse(this string s, string value, int notFoundReturnValue)
        {
            return s.IndexOfElse(value, 0, notFoundReturnValue);
        }

        public static int IndexOfElse(this string s, string value, int startIndex, int notFoundReturnValue)
        {
            int result = s.IndexOf(value, startIndex);
            return result == -1 ? notFoundReturnValue : result;
        }

        public static int LastIndexOfElse(this string s, string value, int notFoundReturnValue)
        {
            return s.LastIndexOfElse(value, s.Length, notFoundReturnValue);
        }

        public static int LastIndexOfElse(this string s, string value, int startIndex, int notFoundReturnValue)
        {
            int result = s.LastIndexOf(value, startIndex);
            return result == -1 ? notFoundReturnValue : result;
        }

        public static string Fill(this char c, int size)
        {
            return new string(Enumerable.Repeat(c, size).ToArray());
        }

        public static string Concatenate<T>(this IEnumerable<T> list)
        {
            return Concatenate(list, "");
        }

        public static string Concatenate<T>(this IEnumerable<T> list, string delimiter)
        {
            if (list == null)
                return "";
            return string.Join(delimiter, list);
        }

        public static string Concatenate<T>(this IEnumerable<T> list, string delimiter, Func<T, string> toString)
        {
            StringBuilder result = new StringBuilder();
            var delim = "";
            foreach (var item in list)
            {
                result.Append(delim);
                delim = delimiter;
                result.Append(toString(item));
            }
            return result.ToString();
        }

        public static string Fill(this string s, int size)
        {
            return Enumerable.Repeat(s, size).Concatenate("");
        }

        public static string Capitalize(this string s)
        {
            return Capitalize(s, false);
        }

        public static string Capitalize(this string s, bool correctCase)
        {
            StringBuilder builder = new StringBuilder();
            if (s.Length > 0)
                builder.Append(char.ToUpper(s[0]));
            for (int i = 1; i < s.Length; i++)
            {
                builder.Append(correctCase ? char.ToLower(s[i]) : s[i]);
            }
            return builder.ToString();
        }

        public static string Decapitalize(this string s)
        {
            return Decapitalize(s, false);
        }

        public static string Decapitalize(this string s, bool correctCase)
        {
            StringBuilder builder = new StringBuilder();
            if (s.Length > 0)
                builder.Append(char.ToLower(s[0]));
            for (int i = 1; i < s.Length; i++)
            {
                builder.Append(correctCase ? char.ToLower(s[i]) : s[i]);
            }
            return builder.ToString();
        }

        public static string DecamelCase(this string s)
        {
            if (s.Length == 0)
                return s;

            StringBuilder buffer = new StringBuilder();
            char[] characters = s.ToCharArray();

            bool wasLowercase = false;
            for (int i = 0; i < characters.Length; i++)
            {
                char character = characters[i];
                if (wasLowercase && char.IsUpper(character))
                {
                    buffer.Append(' ');
                    wasLowercase = false;
                }
                else if (!wasLowercase && char.IsLower(character))
                    wasLowercase = true;

                buffer.Append(character);
            }
            return buffer.ToString();
        }

        public static string Deunderscore(this string s)
        {
            if (s.Length == 0)
                return s;

            var buffer = new StringBuilder();
            var characters = s.ToCharArray();

            var wasUnderscore = false;
            foreach (char character in characters)
            {
                var c = character;
                if (c == '_')
                {
                    buffer.Append(' ');
                    wasUnderscore = true;
                    continue;
                }
                else if (wasUnderscore)
                {
                    c = char.ToUpper(c);
                    wasUnderscore = false;
                }
                buffer.Append(c);
            }
            return buffer.ToString();
        }

        public static string Underscore(this string s)
        {
            if (s.Length == 0)
                return s;

            var buffer = new StringBuilder();
            var characters = s.ToCharArray();

            var wasLowercase = false;
            for (var i = 0; i < characters.Length; i++)
            {
                var character = characters[i];
                if (wasLowercase && char.IsUpper(character))
                {
                    buffer.Append('_');
                    wasLowercase = false;
                }
                else if (!wasLowercase && char.IsLower(character))
                    wasLowercase = true;

                buffer.Append(character);
            }
            return buffer.ToString();
        }

        public static int GetLengthOfCurrentLine(this string s, int position)
        {
            int lineLength = 0;
            for (int i = position - 1; i >= 0; i--)
            {
                if (s[i] == '\r' || s[i] == '\n')
                    break;
                else
                    lineLength++;
            }
            for (int i = position; i < s.Length; i++)
            {
                if (s[i] == '\r' || s[i] == '\n')
                    break;
                else
                    lineLength++;
            }
            return lineLength;
        }

        public static string GetLine(this string s, int offset, out int lineOffset)
        {
            if (offset >= s.Length)
            {
                string result = GetLine(s, s.Length - 1, out lineOffset);
                lineOffset = result.Length + (offset - s.Length) + 1;
                return result;
            }

            int start = s.LastIndexOfAny(new[] { '\n', '\r' }, offset);
            if (start == -1)
                start = 0;
            else if (start < offset)    // If start == offset, then the offset is at a line break.   The code below will adjust for that and get the previous line
                start++;

            int end = s.IndexOfAny(new[] { '\n', '\r' }, offset);
            if (end == -1)
                end = s.Length;

            if (start == end && offset > 0)
            {
                string result = GetLine(s, offset - 1, out lineOffset);
                lineOffset = result.Length + 1;
                return result;
            }

            lineOffset = offset - start;
            return s.Copy(start, end);
        }

        public static string Copy(this string s, int startIndex)
        {
            return s.Substring(startIndex, s.Length - startIndex);
        }

        public static string Copy(this string s, int startIndex, int endIndex)
        {
            return s.Substring(startIndex, endIndex - startIndex);
        }

        public static string ChopStart(this string s, int count)
        {
            return s.Copy(count);
        }

        public static string ChopStart(this string s, string start)
        {
            if (!s.StartsWith(start))
                return s;
            return s.Substring(start.Length);
        }

        public static string ChopEnd(this string s, int count)
        {
            return s.Copy(0, s.Length - count);
        }

        public static bool EndsWith(this string s, string value, bool ignoreCase)
        {
            if (ignoreCase)
                return s.EndsWith(value, StringComparison.CurrentCultureIgnoreCase);
            else
                return s.EndsWith(value, StringComparison.CurrentCulture);
        }

        public static IEnumerable<char> RangeTo(this char startCharacter, char endCharacter)
        {
            if (startCharacter == '(' || endCharacter == ')')
            {
                foreach (char c in new[] { '-', '\'', '\\', '`', '~', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '_', '=', '+', '[', ']', '{', '}', '|', ';', ':', '\"', '<', '>', ',', '.', '/', '?' })
                    yield return c;
            }
            else if ((startCharacter == 'A' && endCharacter == 'z') || (startCharacter == 'a' && endCharacter == 'Z'))
            {
                foreach (char c in 'A'.RangeTo('Z').Union('a'.RangeTo('z')))
                    yield return c;
            }
            else
            {
                for (char c = startCharacter; c <= endCharacter; c++)
                    yield return c;                
            }
        }

        public static string Print(this string s, params object[] args)
        {
            return string.Format(s, args);
        }

        public static string GetCharacterName(this char c)
        {
            switch (c)
            {
                case '-':
                    return "dash";
                case '?':
                    return "question";
                case '!':
                    return "exclamation";
                case '@':
                    return "at";
                case '#':
                    return "pound";
                case '$':
                    return "dollar";
                case '%':
                    return "percent";
                case '^':
                    return "caret";
                case '*':
                    return "asterisk";
                case '(':
                    return "left-paren";
                case ')':
                    return "right-paren";
                case '[':
                    return "left-square-bracket";
                case ']':
                    return "right-square-bracket";
                case '<':
                    return "left-angle-bracket";
                case '>':
                    return "right-angle-bracket";
                case '/':
                    return "slash";
                case '\\':
                    return "backslash";
                case '{':
                    return "left-brace";
                case '}':
                    return "right-brace";
                case '_':
                    return "underscore";
                case '+':
                    return "plus";
                case '=':
                    return "equals";
                case '`':
                    return "tick";
                case '~':
                    return "tilde";
                case ',':
                    return "comma";
                case ':':
                    return "colon";
                case '.':
                    return "period";
                case '|':
                    return "pipe";
                case '\'':
                    return "single-quote";
                case '\"':
                    return "double-quote";
                case ';':
                    return "semicolon";
                default:
                    return c.ToString();
            }
        }

        public static bool EndsWith(this StringBuilder builder, string endsWith)
        {
            for (var i = 0; i < endsWith.Length; i++)
            {
                if (i > builder.Length)
                    return false;
                char c = endsWith[i];
                char compareTo = builder[builder.Length - 1 - i];
                if (c != compareTo)
                    return false;
            }
            return true;
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static int CountTokens(this string s, string token)
        {
            return s.Split(token).Length;
        }

        public static string ChopEnd(this string s, string end)
        {
            if (!s.EndsWith(end))
                return s;
            return s.Substring(0, s.Length - end.Length);
        }

        public static string[] Split(this string s, string delimiter)
        {
            return s.Split(new[] { delimiter }, StringSplitOptions.None);
        }

        public static int IndexOfAny(this string s, params char[] any)
        {
            return s.IndexOfAny(any);
        }

        public static int IndexOfNotAny(this string s, params char[] notAny)
        {
            int index;
            for (index = 0; index < s.Length; index++)
                if (!notAny.Contains(s[index]))
                    break;

            if (index >= s.Length)
                return -1;

            return index;
        }

        public static string NullIfEmpty(this string s)
        {
            return s == "" ? null : s;
        }

        public static string EmptyIfNull(this string s)
        {
            return s ?? "";
        }
        
        public static string Slice(this string s, int startIndex, int endIndex)
        {
            return s.Substring(startIndex, endIndex - startIndex);
        }

		/// <summary>
		/// Returns a substring from the start of the string up to the first occurence of the given end.
		/// The end is not included.  If the given end is not found then the entire string is returned.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="end"></param>
		/// <param name="includeEnd"></param>
		/// <returns></returns>
		public static string SubstringTo(this string s, string end, bool includeEnd = false)
		{
			int index = s.IndexOf(end);
			if (index == -1) return s;
			if (includeEnd) index += end.Length;
			return s.Substring(0, index);
		}

		/// <summary>
		/// Returns a substring from the first occurence of the given start to the end of the string.
		/// The start is not included.  If the given start is not found then the entire string is returned.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="start"></param>
		/// <param name="includeStart"></param>
		/// <returns></returns>
		public static string SubstringFrom(this string s, string start, bool includeStart = false)
		{
			int index = s.IndexOf(start);
			if (index == -1) return s;
			if (!includeStart) index += start.Length;
			return s.Substring(index);
		}

		/// <summary>
		/// Returns a substring from the start of the string up to the last occurence of the given end.
		/// The end is not included.  If the given end is not found then the entire string is returned.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="end"></param>
		/// <param name="includeEnd"></param>
		/// <returns></returns>
		public static string SubstringToLast(this string s, string end, bool includeEnd = false)
		{
			int index = s.LastIndexOf(end);
			if (index == -1) return s;
			if (includeEnd) index += end.Length;
			return s.Substring(0, index);
		}

		/// <summary>
		/// Returns a substring from the last occurence of the given start to the end of the string.
		/// The start is not included.  If the given start is not found then the entire string is returned.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="start"></param>
		/// <param name="includeStart"></param>
		/// <returns></returns>
		public static string SubstringFromLast(this string s, string start, bool includeStart = false)
		{
			int index = s.LastIndexOf(start);
			if (index == -1) return s;
			if (!includeStart) index += start.Length;
			return s.Substring(index);
		}

        public static string Take(this string s, int length, bool allowWordBreak = true)
        {
            int count = length;
            if (count > s.Length)
                count = s.Length;

            if (!allowWordBreak)
                for (int i = count - 1; i >= 0; i--)
                    if (s[i] == ' ')
                    {
                        count = i;
                        break;
                    }

            return s.Substring(0, count);
        }

        public static string Skip(this string s, int length, bool allowWordBreak = true)
        {
            int index = length;
            if (index >= s.Length)
                index = s.Length - 1;
            if (!allowWordBreak)
                for (int i = index; i >= 0; i--)
                    if (s[i] == ' ')
                    {
                        index = i;
                        break;
                    }

            return s.Substring(Math.Max(0, index));
        }

        public static string Summarize(this string s, int length)
        {
            if (s.Length <= length)
                return s;
            else
                return s.Substring(0, length - 3) + "...";
        }

        public static string[] Split(this string s, params string[] delimiters)
        {
            return s.Split(delimiters, StringSplitOptions.None);
        }

        public static string Summarize<T>(this IEnumerable<T> source, string delimiter = ", ", int takeCount = 3, string suffixIfMoreThanTakeCount = "...")
        {
            using (var enumerator = source.GetEnumerator())
            {
                StringBuilder result = new StringBuilder();
                for (int i = 0; i < takeCount; i++)
                {
                    if (!enumerator.MoveNext())
                        return result.ToString();
                    if (result.Length > 0)
                        result.Append(delimiter);
                    result.Append(enumerator.Current.ToString());
                }
                if (enumerator.MoveNext())
                    result.Append(suffixIfMoreThanTakeCount);
                return result.ToString();                
            }
        }

        public static string UpTo(this string s, char c)
        {
            int index = s.IndexOf(c);
            if (index == -1)
                index = s.Length;
            return s.Substring(0, index);
        }

        public static string DeleteAfter(this string s, string search)
        {
            int index = s.IndexOf(search);
            if (index == -1)
                index = s.Length;
            else 
                index += search.Length;
            return s.Substring(0, index);
        }

        public static int NthIndexOf(this string s, string search, int n)
        {
            int index = -search.Length;
            for (int i = 0; i <= n; i++)
            {
                index += search.Length;
                index = s.IndexOf(search, index);                
                if (index == -1)
                    break;
            }
            return index;
        }

        public static string[] SplitAt(this string s, string delimiter, bool includeDelimiter = false)
        {
            int index = s.IndexOf(delimiter);
            if (index == -1)
                return new[] { s, "" };
            
            var part1 = s.Substring(0, index);
            if (!includeDelimiter)
                index += delimiter.Length;

            var part2 = s.Substring(index);

            return new[] { part1, part2 };
        }

		public static string GutExcessWhitespace(this string s) 
		{
			StringBuilder builder = new StringBuilder();
			char lastChar = '\u0000';
			foreach (var c in s)
			{
				bool isValid = true;
				if (c == lastChar && c == ' ')
					isValid = false;
				
				lastChar = c;
				if (!isValid)
					continue;
				
				builder.Append(c);
			}
			return builder.ToString();
		}

        public static string HtmlDecode(this string s)
        {
            return s.Replace("&hellip;", "...");
        }
        
        public static string RemoveIndent(this string s)
        {
            return string.Join("\n", s.Split('\r', '\n').Select(x => x.TrimStart()));
        }

        public static bool IsTrue(this string s)
        {
            return s != null && (s == "true" || s == "True" || s == "Yes" || s == "yes" || s == "On" || s == "on");
        }

        public static string Expand(this string s, object args)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c == '\\')
                {
                    i++;
                    builder.Append(s[i]);
                }
                else if (c == '{')
                {
                    i++;
                    var endIndex = s.IndexOf('}', i);
                    var identifier = s.Substring(i, endIndex - i);
                    i = endIndex;
                    var value = args.GetValue(identifier);
                    builder.Append(value);
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        public static bool IsValidEmailAddress(this string s) { 
            /* taken from: 
             * http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx/
             */
            string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|" 
                + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)" 
                + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            if (string.IsNullOrWhiteSpace(s) || !regex.IsMatch (s)) {
                return false;
            }

            return true;
        }

        public static bool IsValidPhoneNumber(this string s)
        {
            /* matching E.164 standards http://en.wikipedia.org/wiki/E.164 */
            string pattern = @"^\+?[1-9]\d{1,14}$";

            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            if (string.IsNullOrWhiteSpace(s) || !regex.IsMatch(s))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Compare two strings and return the index of the first difference.  
        /// Return -1 if the strings are equal.
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static int DiffersAtIndex(this string s1, string s2)
        {
            int index = 0;
            int min = Math.Min(s1.Length, s2.Length);
            while (index < min && s1[index] == s2[index])
            {
                index++;
            }

            return (index == min && s1.Length == s2.Length) ? -1 : index;
        }
	}
}
