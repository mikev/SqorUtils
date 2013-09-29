using System;

namespace Sqor.Utils.Delegates
{
    public static class Actions
    {
        public static Action Nop
        {
            get { return () => {}; }
        }
    
        public static void Nop1<T>(T parameter1)
        {
        }
    }
}

