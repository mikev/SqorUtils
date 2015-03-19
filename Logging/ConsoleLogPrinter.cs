using System;
using System.Diagnostics;

namespace Sqor.Utils.Logging
{
    public class ConsoleLogPrinter : ILogPrinter
    {
        public void Print(Logger logger, MessageType type, string message)
        {
            //Console.Write(message);
            Debug.WriteLine(message);
        }
    }
}
