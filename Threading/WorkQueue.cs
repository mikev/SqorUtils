using System;

namespace Sqor.Utils.Threading
{
    public class WorkQueue : WorkerThread
    {
        public WorkQueue() : base("SqorWorkerThread")
        {
        }
    }
}

