using System.Text;
using System.Threading;

namespace Sqor.Utils.Logging
{
    public class BasicLogFormatter : ILogFormatter
    {
        public string Format(LogContext context)
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(Thread.CurrentThread.Name))
            {
                builder.Append(Thread.CurrentThread.Name);
            }
            else
            {
                builder.Append("Thread(" + Thread.CurrentThread.GetHashCode() + ")");
            }
            builder.Append(" - ");
            builder.Append(context.TargetType.Name);
            builder.Append(" - ");
            switch (context.Severity) 
            {
                case MessageType.Information: 
                    builder.Append("INFO");
                    break;
                case MessageType.Warning:
                    builder.Append("WARNING");
                    break;
                case MessageType.Error:
                    builder.Append("ERROR");
                    break;
            }
            if (context.Message != null)
            {
                builder.Append(" - ");
                builder.Append(context.Message);                
            }
            if (context.Exception != null)
            {
                builder.AppendLine();
                builder.Append(context.Exception);
            }
            builder.AppendLine();

            return builder.ToString();
        }
    }
}
