using System;

namespace Sqor.Utils.Logging
{
    public class ConsoleLogPrinter : ILogPrinter
    {
        public void Print(Logger logger, MessageType type, string message)
        {
            Console.Write(message);
        }
    }
}
