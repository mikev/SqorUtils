using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Sqor.Utils.Threading
{
    public class AsyncLazy<T> : Lazy<Task<T>> 
    { 
//        public AsyncLazy(Func<T> valueFactory) : 
//            base(() => Task.Factory.StartNew(valueFactory)) 
//        { 
//        }
    
        public AsyncLazy(Func<Task<T>> taskFactory) : 
            base(taskFactory) 
        { 
        } 
    
        public TaskAwaiter<T> GetAwaiter() 
        { 
            return Value.GetAwaiter(); 
        } 
    }
}