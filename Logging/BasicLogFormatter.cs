using System.Text;
using System.Threading;

namespace Sqor.Utils.Logging
{
    public class BasicLogFormatter : ILogFormatter
    {
        public string Format(LogContext context)
        {
            StringBuilder builder = new StringBuilder();
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
            builder.Append(" - ");
            builder.Append(context.TargetType.Name);
            if (!Thread.CurrentThread.IsThreadPoolThread && !string.IsNullOrEmpty(Thread.CurrentThread.Name))
            {
                builder.Append(" - " + Thread.CurrentThread.Name);
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