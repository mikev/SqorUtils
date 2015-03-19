namespace Sqor.Utils.Logging
{
    public class NullLogPrinter : ILogPrinter
    {
        public void Print(Logger logger, MessageType type, string message)
        {
        }
    }
}