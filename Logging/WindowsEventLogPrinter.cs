using System;
using System.Diagnostics;
using Microsoft.Win32;

#if !MONOTOUCH && !MONODROID

namespace Sqor.Utils.Logging
{
    public class WindowsEventLogPrinter : ILogPrinter
    {
        public string Source { get; set; }

        public WindowsEventLogPrinter(string source)
        {
            Source = source;

            // Check if source already exists.  Because we are requesting write access, this
            // will fail unless you give "NETWORK SERVICE" (or IIS_IUSRS) full control of the below key.  Just
            // do this once on the server and never think about it again.
            var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\EventLog\Application", true);
            var sourceKey = key.OpenSubKey(source);
            if (sourceKey == null)
            {
                key.CreateSubKey(source);
            }
        }

        public void Print(Logger logger, MessageType type, string message)
        {
            EventLogEntryType entryType;
            switch (type)
            {
                case MessageType.Error:
                    entryType = EventLogEntryType.Error;
                    break;
                case MessageType.Warning:
                    entryType = EventLogEntryType.Warning;
                    break;
                case MessageType.Information:
                    entryType = EventLogEntryType.Information;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            EventLog.WriteEntry(Source, "Test", entryType);
        }
    }
}

#endif