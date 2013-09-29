using System;

namespace Sqor.Utils.Logging
{
    public struct LogContext
    {
        public MessageType Severity { get; set; }
        public object Target { get; private set; }
        public Type TargetType { get; private set; }
        public string Message { get; private set; } 
        public Exception Exception { get; private set; }

        public LogContext(MessageType severity, object target, Type targetType, string message, Exception exception)
            : this()
        {
            Severity = severity;
            Target = target;
            TargetType = targetType;
            Message = message;
            Exception = exception;
        }
    }
}