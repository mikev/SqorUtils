using System;
using System.Threading.Tasks;

namespace Sqor.Utils.Threading
{
    public static class TaskExtensions
    {
        public static T Get<T>(this Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }
}

