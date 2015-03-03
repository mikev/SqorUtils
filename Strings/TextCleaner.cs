using System.Web;
using HtmlAgilityPack;

namespace Sqor.Utils.Strings
{
    public class TextCleaner
    {
        private int paragraphLimit;
        private int numberOfParagraphs = 1;
        private LimitedWhitespaceStringBuilder builder = new LimitedWhitespaceStringBuilder();

        public TextCleaner(int paragraphLimit)
        {
            this.paragraphLimit = paragraphLimit;
        }

        /// <summary>
        /// Strips text of HTML and converts paragraphs into double new-lines.
        /// </summary>
        /// <returns></returns>
        public static string CleanText(string s, int paragraphLimit = 3)
        {
            if (s == null)
                return null;

            var document = new HtmlDocument();
            document.LoadHtml(s);

            var cleaner = new TextCleaner(paragraphLimit);
            cleaner.PrintNode(document.DocumentNode);
            return cleaner.builder.ToString().Trim();
        }

        private bool PrintNode(HtmlNode node)
        {
            if (node is HtmlTextNode)
            {
                var textNode = (HtmlTextNode)node;
                var text = textNode.Text;

                text = HttpUtility.HtmlDecode(text);

                builder.Append(text);
            }

            foreach (var child in node.ChildNodes)
            {
                if (!PrintNode(child))
                    return false;
            }

            if (node.Name == "br")
            {
                builder.AppendLine();
            }
            if (node.Name == "p")
            {
                numberOfParagraphs++;
                if (numberOfParagraphs > paragraphLimit)
                    return false;
                builder.AppendLine();
                builder.AppendLine();
            }
            return true;
        }
    }
}
