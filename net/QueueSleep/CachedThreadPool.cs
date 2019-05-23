using System.Collections.Concurrent;
using System.Threading;

namespace QueueSleep
{
    public static class CachedThreadPool
    {
        private static int _threads;

        private static readonly AutoResetEvent _managementEvent;
        private static readonly Thread _managementThread;

        private static ConcurrentBag<ThreadState> _idleThreads = new ConcurrentBag<ThreadState>();
        private static ConcurrentQueue<WaitCallback> _queuedCallbacks = new ConcurrentQueue<WaitCallback>();

        public static int MaxThreads { get; set; } = int.MaxValue;

        static CachedThreadPool()
        {
            _managementEvent = new AutoResetEvent(false);

            _managementThread = new Thread(ProcessQueuedCallbacks);
            _managementThread.Start();
        }

        public static void ProcessQueuedCallbacks()
        {
            while (true)
            {
                _managementEvent.WaitOne();

                while (_queuedCallbacks.TryDequeue(out var callback))
                {
                    if (_idleThreads.TryTake(out ThreadState threadState))
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

                            // TODO: Restore to front of queue instead of back (for fairness)
                            _queuedCallbacks.Enqueue(callback);

                            // Wait for next event signal
                            break;
                        }
                    }
                }
            }
        }

        public static bool QueueUserWorkItem(WaitCallback callback)
        {
            _queuedCallbacks.Enqueue(callback);
            _managementEvent.Set();
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
                    _idleThreads.Add(this);
                    _managementEvent.Set();
                }
            }
        }
    }
}
