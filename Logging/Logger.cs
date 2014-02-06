using System;
using System.Diagnostics;

namespace Sqor.Utils.Logging
{
    /// <summary>
    /// These are designed to exist one per app domain.  In other words, a singleton.  Each application endpoint should configure 
    /// this class accordingly by specifying a printer.  It defaults to the console.
    /// </summary>
    public class Logger
    {
        private static readonly Logger instance = new Logger();

        public static Logger Instance
        {
            get { return instance; }
        }

        public ILogPrinter Printer { get; set; }
        public ILogFormatter Formatter { get; set; }
        public bool IsActive { get; set; }
        private object lockObject = new object();

        public Logger()
        {
            Printer = new ConsoleLogPrinter();
            IsActive = true;
        }

        public LogSuspensinon SuspendLogging()
        {
            lock (lockObject)
            {
                IsActive = false;                
            }
            return new LogSuspensinon(this);
        }

        public void ResumeLogging()
        {
            lock (lockObject)
            {
                IsActive = true;
            }
        }

        public void Log(LogContext context)
        {
            lock (lockObject)
            {
                if (!IsActive || Printer == null || Formatter == null)
                    return;
                Printer.Print(this, context.Severity, Formatter.Format(context));
            }
        }        

        public class LogSuspensinon : IDisposable
        {
            private Logger logger;

            public LogSuspensinon(Logger logger)
            {
                this.logger = logger;
            }

            public void Dispose()
            {
                logger.ResumeLogging();
            }
        }
    }
}
