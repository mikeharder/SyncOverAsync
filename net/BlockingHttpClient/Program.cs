using CommandLine;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;

namespace BlockingHttpClient
{
    class Program
    {
        private static HttpClient _httpClient = new HttpClient();
        private static Stopwatch _stopwatch = Stopwatch.StartNew();

        private static long _requestsQueued;
        private static long _requestsExecuted;
        private static long _responses;
        private static long _responseLatencyTicks;

        private class Options
        {
            [Option('u', "uri", Required = true)]
            public Uri Uri { get; set; }

            [Option('r', "requestsPerSecond", Default = 10)]
            public int RequestsPerSecond { get; set; }

            [Option('s', "syncOverAsync", Default = false)]
            public bool SyncOverAsync { get; set; }

            [Option('w', "minWorkerThreads")]
            public int? MinWorkerThreads { get; set; }

            [Option('c', "minCompletionPortThreads")]
            public int? MinCompletionPortThreads { get; set; }
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

            ThreadPool.GetMinThreads(out var previousMinWorkerThreads, out var previousMinCompletionPortThreads);
            ThreadPool.SetMinThreads(
                options.MinWorkerThreads ?? previousMinWorkerThreads,
                options.MinCompletionPortThreads ?? previousMinCompletionPortThreads
                );

            ThreadPool.GetMinThreads(out var minWorkerThreads, out var minCompletionPortThreads);
            ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);
            Console.WriteLine($"ThreadPool.GetMinThreads(): {minWorkerThreads}, {minCompletionPortThreads}");
            Console.WriteLine($"ThreadPool.GetMaxThreads(): {maxWorkerThreads}, {maxCompletionPortThreads}");

            Console.WriteLine();

            var writeResultsThread = new Thread(WriteResults);
            writeResultsThread.Start();

            RunTest(options.Uri, options.RequestsPerSecond, options.SyncOverAsync);

            writeResultsThread.Join();

            return 0;
        }

        private static void RunTest(Uri uri, int requestsPerSecond, bool syncOverAsync)
        {
            while (true)
            {
                if (Interlocked.Read(ref _requestsQueued) < requestsPerSecond * _stopwatch.Elapsed.TotalSeconds)
                {
                    Interlocked.Increment(ref _requestsQueued);
                    var start = _stopwatch.ElapsedTicks;

                    ThreadPool.QueueUserWorkItem((state) => ExecuteRequest(uri, syncOverAsync, start));
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                }
            }
        }

        private static async Task ExecuteRequest(Uri uri, bool syncOverAsync, long startTicks)
        {
            Interlocked.Increment(ref _requestsExecuted);

            var task = _httpClient.GetAsync(uri);
            if (syncOverAsync)
            {
                await Task.Yield();
                task.Wait();
            }
            else
            {
                await task;
            }
            var end = _stopwatch.ElapsedTicks;

            Interlocked.Add(ref _responseLatencyTicks, end - startTicks);
            Interlocked.Increment(ref _responses);

            task.Result.Dispose();
        }

        private static void WriteResults()
        {
            var lastRequestsQueued = (long)0;
            var lastResponses = (long)0;
            var lastResponseLatencyTicks = (long)0;
            var lastElapsed = TimeSpan.Zero;

            while (true)
            {
                Thread.Sleep(1000);

                var requestsQueued = Interlocked.Read(ref _requestsQueued);
                var currentRequestsQueued = requestsQueued - lastRequestsQueued;
                lastRequestsQueued = requestsQueued;

                var responses = Interlocked.Read(ref _responses);
                var currentResponses = responses - lastResponses;
                lastResponses = responses;

                var responseLatencyTicks = Interlocked.Read(ref _responseLatencyTicks);
                var currentResponseLatencyTicks = responseLatencyTicks - lastResponseLatencyTicks;
                lastResponseLatencyTicks = responseLatencyTicks;

                var elapsed = _stopwatch.Elapsed;
                var currentElapsed = elapsed - lastElapsed;
                lastElapsed = elapsed;

                WriteResult(requestsQueued, Interlocked.Read(ref _requestsExecuted), responses,
                    currentRequestsQueued, currentResponses, currentResponseLatencyTicks, currentElapsed);
            }
        }

        private static void WriteResult(long totalRequestsQueued, long totalRequestsExecuted, long totalResponses,
            long currentRequestsQueued, long currentResponses, long currentResponseLatencyTicks, TimeSpan currentElapsed)
        {
            var currentResponseLatencyMs = ((double)currentResponseLatencyTicks / Stopwatch.Frequency) * 1000;
            var threads = Process.GetCurrentProcess().Threads.Count;

            Console.WriteLine(
                $"{DateTime.UtcNow.ToString("o")}" +
                $"\tTot Req\t{totalRequestsQueued}" +
                $"\tPen Req\t{totalRequestsQueued - totalRequestsExecuted}" +
                $"\tOut Req\t{totalRequestsExecuted - totalResponses}" +
                $"\tTot Rsp\t{totalResponses}" +
                $"\tCur Q/S\t{Math.Round(currentRequestsQueued / currentElapsed.TotalSeconds)}" +
                $"\tCur R/S\t{Math.Round(currentResponses / currentElapsed.TotalSeconds)}" +
                $"\tCur Lat\t{Math.Round(currentResponseLatencyMs / currentResponses, 0)}ms" +
                $"\tThreads\t{threads}"
            );
        }
    }
}
