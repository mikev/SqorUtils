using System;
using System.Text;

namespace Sqor.Utils.Exceptions
{
    public static class ExceptionExtensions
    {
        public static string ToExceptionReport(this Exception exception)
        {
            StringBuilder builder = new StringBuilder();
            
            var current = exception;
            while (current != null)
            {
                builder.AppendLine(current.ToString());
                current = current.InnerException;
                
                if (current != null)
                    builder.AppendLine("Caused by:");
            }
            
            return builder.ToString();
        }
    }
}

