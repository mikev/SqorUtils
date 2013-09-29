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
        private Thread thread;

        public WorkerThread(string name) 
        {
            thread = new Thread(Run);
            thread.Name = name;
            thread.IsBackground = true;
        }
        
        protected Thread Thread
        {
            get { return thread; }
        }

        public void Start()
        {
            thread.Start();
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
                Queue(Tuple.Create(action, (Action)(() => autoResetEvent.Set())));
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

