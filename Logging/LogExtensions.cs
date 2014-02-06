using System;
using System.Diagnostics;

namespace Sqor.Utils.Logging
{
    public static class LogExtensions
    {
        public static void LogInfo<T>(this T target, string message)
        {
            Logger.Instance.Log(new LogContext(MessageType.Information, target, target.GetType(), message, null));
        }

        public static void LogInfo<T>(this T target, string message, Exception e)
        {
            Logger.Instance.Log(new LogContext(MessageType.Information, target, target.GetType(), message, e));
        }

        public static void LogWarning<T>(this T target, string message, Exception e)
        {
            Logger.Instance.Log(new LogContext(MessageType.Warning, target, target.GetType(), message, e));
        }

        public static void LogError<T>(this T target, string message)
        {
            Logger.Instance.Log(new LogContext(MessageType.Error, target, target.GetType(), message, null));            
        }

        public static void LogError<T>(this T target, string message, Exception e)
        {
            Logger.Instance.Log(new LogContext(MessageType.Error, target, target.GetType(), message, e));
        }
    }
}
