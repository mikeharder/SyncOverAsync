using CommandLine;
using System;
using System.Diagnostics;
using System.Runtime;
using System.Threading;

namespace QueueSleep
{
    class Program
    {
        private static Options _options;

        private static int _minWorkerThreads;

        private static int _queued;
        private static int _started;
        private static int _finished;
        private static readonly ManualResetEvent _done = new ManualResetEvent(false);

        private class Options
        {
            [Option('c', "count", Default = 200)]
            public int Count { get; set; }

            [Option('t', "minWorkerThreads", HelpText = "Value of -1 sets min worker threads to maximum available")]
            public int? MinWorkerThreads { get; set; }

            [Option('p', "threadPool", HelpText = "[ThreadPool|CachedThreadPool]", Default = "ThreadPool")]
            public string ThreadPool { get; set; }

            [Option('s', "printStatistics", Default = true)]
            public bool PrintStatistics { get; set; }
        }

        static int Main(string[] args)
        {
            var parser = new Parser(settings =>
            {
                settings.HelpWriter = Console.Out;
            });

            return parser.ParseArguments<Options>(args).MapResult(
                options => Run(options),
                _ => 1
            );
        }

        private static int Run(Options options)
        {
            _options = options;

            if (_options.MinWorkerThreads.HasValue)
            {
                var minWorkerThreads = options.MinWorkerThreads.Value;

                if (_options.ThreadPool.Equals("ThreadPool", StringComparison.OrdinalIgnoreCase))
                {
                    if (minWorkerThreads == -1)
                    {
                        ThreadPool.GetMaxThreads(out minWorkerThreads, out var _);
                    }

                    ThreadPool.GetMinThreads(out var _, out var minCompletionPortThreads);
                    ThreadPool.SetMinThreads(minWorkerThreads, minCompletionPortThreads);
                }
                else if (_options.ThreadPool.Equals("CachedThreadPool", StringComparison.OrdinalIgnoreCase))
                {
                    if (minWorkerThreads > 0)
                    {
                        CachedThreadPool.MaxThreads = minWorkerThreads;
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Invalid ThreadPool: {_options.ThreadPool}");
                }
            }

            PrintInitialStatus();

            var writeResultsThread = new Thread(() => WriteResults());
            writeResultsThread.Start();

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < _options.Count; i++)
            {
                Interlocked.Increment(ref _queued);
                QueueWorkItem(SleepAndDoWork);
            }
            _done.WaitOne();
            sw.Stop();

            writeResultsThread.Join();

            var rps = _options.Count / sw.Elapsed.TotalSeconds;
            Console.WriteLine($"Executed {_options.Count:n0} work items with {_minWorkerThreads:n0} minWorkerThreads in {sw.Elapsed.TotalSeconds:N2}s for {rps:N0} RPS");

            return 0;
        }

        private static void SleepAndDoWork(object state)
        {
            Interlocked.Increment(ref _started);
            Thread.Sleep(1000);
            var finished = Interlocked.Increment(ref _finished);
            if (finished == _options.Count)
            {
                _done.Set();
            }
        }

        private static bool QueueWorkItem(WaitCallback callback)
        {
            if (_options.ThreadPool.Equals("ThreadPool", StringComparison.OrdinalIgnoreCase))
            {
                return ThreadPool.QueueUserWorkItem(callback);
            }
            else if (_options.ThreadPool.Equals("CachedThreadPool", StringComparison.OrdinalIgnoreCase))
            {
                return CachedThreadPool.QueueUserWorkItem(callback);
            }
            else
            {
                throw new InvalidOperationException($"Invalid ThreadPool: {_options.ThreadPool}");
            }
        }

        private static void WriteResults()
        {
            var lastUserProcessorTime = TimeSpan.Zero;
            var lastPrivilegedProcessorTime = TimeSpan.Zero;
            var lastTotalProcessorTime = TimeSpan.Zero;
            var lastElapsed = TimeSpan.Zero;
            var stopwatch = Stopwatch.StartNew();

            while (_finished < _options.Count)
            {
                var process = Process.GetCurrentProcess();
                var osThreads = process.Threads.Count;

                var userProcessorTime = process.UserProcessorTime;
                var currentUserProcessorTime = userProcessorTime - lastUserProcessorTime;
                lastUserProcessorTime = userProcessorTime;

                var privilegedProcessorTime = process.PrivilegedProcessorTime;
                var currentPrivilegedProcessorTime = privilegedProcessorTime - lastPrivilegedProcessorTime;
                lastPrivilegedProcessorTime = privilegedProcessorTime;

                var totalProcessorTime = process.TotalProcessorTime;
                var currentTotalProcessorTime = totalProcessorTime - lastTotalProcessorTime;
                lastTotalProcessorTime = totalProcessorTime;

                var elapsed = stopwatch.Elapsed;
                var currentElapsed = elapsed - lastElapsed;
                lastElapsed = elapsed;

                Thread.Sleep(1000);
                WriteResult(osThreads, currentUserProcessorTime, currentPrivilegedProcessorTime, currentTotalProcessorTime, currentElapsed);
            }
        }

        private static void WriteResult(int osThreads, TimeSpan currentUserProcessorTime, TimeSpan currentPrivilegedProcessorTime,
            TimeSpan currentTotalProcessorTime, TimeSpan currentElapsed)
        {
            ThreadPool.GetMinThreads(out var minWorkerThreads, out var _);
            ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var _);
            ThreadPool.GetAvailableThreads(out var availableWorkerThreads, out var _);

            var currentElapsedCpuTicks = currentElapsed.Ticks * Environment.ProcessorCount;
            var currentUserProcessorPercentage = ((double)currentUserProcessorTime.Ticks) / currentElapsedCpuTicks;
            var currentPrivilegedProcessorPercentage = ((double)currentPrivilegedProcessorTime.Ticks) / currentElapsedCpuTicks;
            var currentTotalProcessorPercentage = ((double)currentTotalProcessorTime.Ticks) / currentElapsedCpuTicks;

            Console.WriteLine(
                $"{DateTime.UtcNow.ToString("o")}" +
                $"\tQueued\t{_queued}" +
                $"\tStarted\t{_started}" +
                $"\tFinishd\t{_finished}" +
                $"\tTP.Min\t{minWorkerThreads}" +
                $"\tTP.Max\t{maxWorkerThreads}" +
                $"\tTP.Cur\t{maxWorkerThreads - availableWorkerThreads}" +
                $"\tOsThds\t{osThreads}"

            // CPU data may be inaccurate (total > 100% which should be impossible)
            //$"\tUsr CPU\t{currentUserProcessorPercentage:P1}" +
            //$"\tPrv CPU\t{currentPrivilegedProcessorPercentage:P1}" +
            //$"\tTot CPU\t{currentTotalProcessorPercentage:P1}"
            );
        }

        private static void PrintInitialStatus()
        {
#if RELEASE
            Console.WriteLine("Configuration: Release");
#else
            throw new InvalidOperationException("Must be run in 'Release' configuration");
#endif

            if (GCSettings.IsServerGC)
            {
                Console.WriteLine("GC: Server");
            }
            else
            {
                throw new InvalidOperationException("Must be run with server GC");
            }

            ThreadPool.GetMinThreads(out _minWorkerThreads, out var minCompletionPortThreads);
            ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);
            Console.WriteLine($"ThreadPool.GetMinThreads(): {_minWorkerThreads}, {minCompletionPortThreads}");
            Console.WriteLine($"ThreadPool.GetMaxThreads(): {maxWorkerThreads}, {maxCompletionPortThreads}");

            Console.WriteLine();
        }
    }
}
