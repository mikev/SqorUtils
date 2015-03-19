using System;

namespace Sqor.Utils.Logging
{
    public class HybridPrinter : ILogPrinter
    {
        private ILogPrinter[] printers;

        public HybridPrinter(params ILogPrinter[] printers)
        {
            this.printers = printers;
        }

        public void Print(Logger logger, MessageType type, string message)
        {
            foreach (var printer in printers) 
            {
                printer.Print(logger, type, message);
            }
        }
    }
}

