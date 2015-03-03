using System.Text;

namespace Sqor.Utils.Strings
{
    /// <summary>
    /// Prevents more than one whitespace character from being appended in a row. (not 
    /// counting newlines)
    /// </summary>
    public class LimitedWhitespaceStringBuilder 
    {
        private StringBuilder builder = new StringBuilder();
        private bool lastCharWasWhitespace;

        public void Append(string s)
        {
            foreach (var c in s)
            {
                var current = c;
                if (char.IsWhiteSpace(current))
                {
                    current = ' ';
                }
                if (current == ' ')
                {
                    if (lastCharWasWhitespace)
                        continue;
                    lastCharWasWhitespace = true;
                }
                else
                {
                    lastCharWasWhitespace = false;
                }
                builder.Append(current);
            }
        }

        public void AppendLine()
        {
            builder.Append("\n");
            lastCharWasWhitespace = true;
        }

        public override string ToString()
        {
            return builder.ToString();
        }
    }
}
