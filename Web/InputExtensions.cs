using System.Web;
using System.Web.Mvc;
using Sqor.Utils.Dictionaries;

namespace Sqor.Utils.Web
{
    public static class InputExtensions
    {
        public static HtmlString CheckBox(this HtmlHelper html, string name, string value = "true", bool isChecked = false, object
            htmlAttributes = null)
        {
            var tagBuilder = new TagBuilder("input");
            if (htmlAttributes != null)
                tagBuilder.MergeAttributes(htmlAttributes.ToDictionary());
            tagBuilder.MergeAttribute("type", "checkbox");
            tagBuilder.MergeAttribute("name", name, true);
            tagBuilder.MergeAttribute("value", value, true);
            if (isChecked)
                tagBuilder.MergeAttribute("checked", "checked", true);
            return new HtmlString(tagBuilder.ToString());
        }
    }
}
