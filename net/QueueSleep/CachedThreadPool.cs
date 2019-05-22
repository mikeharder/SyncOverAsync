using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace QueueSleep
{
    public static class CachedThreadPool
    {
        private static int _threads;

        private static ConcurrentBag<ThreadState> _idleThreads = new ConcurrentBag<ThreadState>();
        private static ConcurrentQueue<WaitCallback> _queuedCallbacks = new ConcurrentQueue<WaitCallback>();

        public static int MaxThreads { get; set; } = int.MaxValue;

        public static bool QueueUserWorkItem(WaitCallback callback)
        {
            ThreadState threadState;

            if (_idleThreads.TryTake(out threadState))
            {
                threadState.Run(callback);
            }
            else
            {
                var threads = Interlocked.Increment(ref _threads);
                if (threads <= MaxThreads)
                {
                    threadState = new ThreadState();
                    threadState.Run(callback);
                }
                else
                {
                    Interlocked.Decrement(ref _threads);
                    _queuedCallbacks.Enqueue(callback);
                }
            }

            return true;
        }

        public class ThreadState
        {
            private readonly Thread _thread;
            private readonly AutoResetEvent _resetEvent;
            private WaitCallback _callback;

            public ThreadState()
            {
                _resetEvent = new AutoResetEvent(false);

                _thread = new Thread(ExecuteCallbacks);
                _thread.Start();
            }

            public void Run(WaitCallback callback)
            {
                _callback = callback;
                _resetEvent.Set();
            }

            private void ExecuteCallbacks()
            {
                while (true)
                {
                    _resetEvent.WaitOne();
                    _callback(null);
                    _callback = null;

                    if (_queuedCallbacks.TryDequeue(out var nextCallback))
                    {
                        Run(nextCallback);
                    }
                    else
                    {
                        _idleThreads.Add(this);
                    }
                }
            }
        }
    }
}
