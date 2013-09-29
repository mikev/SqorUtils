namespace Sqor.Utils.Logging
{
    public class FileLogPrinter : ILogPrinter
    {
        private IFileManager fileManager;

        public FileLogPrinter(IFileManager fileManager)
        {
            this.fileManager = fileManager;
        }

        public void Print(Logger logger, MessageType type, string message)
        {
            fileManager.Output.Write(message);
            fileManager.Output.Flush();
        }
    }
}