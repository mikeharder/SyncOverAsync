using CommandLine;
using System;
using System.Diagnostics;
using System.Runtime;
using System.Threading;

namespace QueueSleep
{
    class Program
    {
        private static int _queued;
        private static int _started;
        private static int _finished;

        private class Options
        {
            [Option('c', "count", Default = 100)]
            public int Count { get; set; }
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
            Console.WriteLine();

            var writeResultsThread = new Thread(() => WriteResults(options.Count));
            writeResultsThread.Start();

            for (int i = 0; i < options.Count; i++)
            {
                Interlocked.Increment(ref _queued);
                QueueWorkItem(SleepAndDoWork);
            }

            writeResultsThread.Join();

            return 0;
        }

        private static void SleepAndDoWork(object state)
        {
            Interlocked.Increment(ref _started);
            Thread.Sleep(1000);
            Interlocked.Increment(ref _finished);
        }

        private static bool QueueWorkItem(WaitCallback callback)
        {
            return ThreadPool.QueueUserWorkItem(callback);
        }

        private static void WriteResults(int count)
        {
            while (_finished < count)
            {
                Thread.Sleep(1000);
                WriteResult();
            }
        }

        private static void WriteResult()
        {
            var osThreads = Process.GetCurrentProcess().Threads.Count;
            ThreadPool.GetMinThreads(out var minWorkerThreads, out var _);
            ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var _);
            ThreadPool.GetAvailableThreads(out var availableWorkerThreads, out var _);

            Console.WriteLine(
                $"{DateTime.UtcNow.ToString("o")}" +
                $"\tQueued\t{_queued}" +
                $"\tStarted\t{_started}" +
                $"\tFinished\t{_finished}" +
                $"\tTP.Min\t{minWorkerThreads}" +
                $"\tTP.Max\t{maxWorkerThreads}" +
                $"\tTP.Avail\t{availableWorkerThreads}" +
                $"\tOsThds\t{osThreads}"
            );
        }

    }
}
