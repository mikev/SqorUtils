using System;

namespace Sqor.Utils.Ios
{
    /// <summary>
    /// Using these extensions is preferable than checking for null inline because to do
    /// that properly (in a thread safe sense) you need to declare a variable to store it
    /// (in case it changes (goes null) between the two reads).
    /// </summary>
    public static class EventExtensions
    {
        public static void Fire(this Action @event)
        {
            if (@event != null)
                 @event();
        }

        public static void Fire<T>(this T @event, Action<T> pinned)
        {
            if (@event != null)
                pinned(@event);
        }
    }
}

