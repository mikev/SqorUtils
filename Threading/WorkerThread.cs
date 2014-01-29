using System;
using System.Threading;
using System.Collections.Generic;
using Sqor.Utils.Logging;
using System.Linq;
using Sqor.Utils.Lists;
using Sqor.Utils.Delegates;

namespace Sqor.Utils.Threading
{
    public class WorkerThread 
    {
        private AutoResetEvent waitLock = new AutoResetEvent(false);
        private List<Tuple<Action, Action>> queue = new List<Tuple<Action, Action>>();
        private object lockObject = new object();
        private List<Thread> threadPool = new List<Thread>();

        public WorkerThread(string name, int poolSize = 4) 
        {
            for (var i = 0; i < poolSize; i++)
            {
                var thread = new Thread(Run);
                thread.Name = name + ":" + (i + 1);
                thread.IsBackground = true;
                threadPool.Add(thread);
            }
        }
        
        protected Thread[] Threads
        {
            get { return threadPool.ToArray(); }
        }

        public void Start()
        {
            foreach (var thread in threadPool)
            {
               thread.Start();
            }
        }

        public void Queue(Action action)
        {
            Queue(Tuple.Create(action, Actions.Nop));
        }

        public void Queue(Tuple<Action, Action> action)
        {
            lock (lockObject)
            {
                queue.Enqueue(action);
            }
            waitLock.Set();
        }
        
        public void JumpQueue(Action action)
        {
            lock (lockObject)
            {
                queue.JumpQueue(Tuple.Create(action, Actions.Nop));
            }
            waitLock.Set();
        }
        
        public void JumpQueue(Tuple<Action, Action> action)
        {
            lock (lockObject)
            {
                queue.JumpQueue(action);
            }
            waitLock.Set();
        }

        public void Execute(Action action)
        {
            using(var autoResetEvent = new AutoResetEvent(false)) 
            {
                Queue(Tuple.Create(action, (Action)(() => autoResetEvent.Set()))); // (It's not disposed, if you're seeing a resharper message you should disable that warning)
                autoResetEvent.WaitOne();
            }
        }
        
        private void Run()
        {
            while (true)
            {
                waitLock.WaitOne();
                Tuple<Action, Action> action;
                do
                {
                    lock (lockObject)
                    {
                        action = queue.Any() ? queue.Dequeue() : null;
                    }
                    if (action != null)
                    {
                        try
                        {
                            this.LogInfo("Invoking action on " + Thread.CurrentThread.Name);
                            action.Item1();
                        }
                        catch (Exception e)
                        {
                            this.LogInfo("Error running job.", e);
                        }
                        finally 
                        {
                            action.Item2();
                        }
                    }
                } 
                while (action != null);
            }
        }
    }
}

