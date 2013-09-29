using System;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;

#if MONOTOUCH

namespace Sqor.Utils.Net
{
    public static class NSUrlExtensions
    {
        public static int? GetPort(this NSUrl url)
        {
            NSValue port;
            if (typeof(NSUrl) == url.GetType())
            {
                port = NSNumber.ValueFromPointer(Messaging.IntPtr_objc_msgSend(url.Handle, Selector.GetHandle("port")));
            }
            else 
            {
                port = NSNumber.ValueFromPointer(Messaging.IntPtr_objc_msgSendSuper(url.SuperHandle, Selector.GetHandle("port")));
            }
            using (port)
            {
                if (port.PointerValue == IntPtr.Zero)
                    return null;
                else
                    return (int)(NSNumber)port.NonretainedObjectValue;
            }
        }

//        private static FieldInfo FieldIsDirectBinding = typeof(NSObject).GetField("IsDirectBinding"

//        private static bool IsDirectBinding(this NSObject obj)
//        {
//
//        }
    }
}

#endif