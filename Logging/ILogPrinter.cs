namespace Sqor.Utils.Logging
{
    public interface ILogPrinter
    {
        void Print(Logger logger, MessageType type, string message);
    }
}