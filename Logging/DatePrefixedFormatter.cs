using System;

namespace Sqor.Utils.Logging
{
    public class DatePrefixedFormatter : ILogFormatter
    {
        private ILogFormatter target;
        private string formatString;

        public DatePrefixedFormatter(ILogFormatter target, string formatString = "yyyy-MM-dd hh:mm:ss")
        {
            this.target = target;
            this.formatString = formatString;
        }

        public string Format(LogContext context)
        {
            return DateTime.Now.ToString(formatString) + " - " + target.Format(context);
        }
    }
}
