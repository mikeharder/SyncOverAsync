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

        private static long _requests;
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

            var writeResultsTask = WriteResults();

            RunTest(options.Uri, options.RequestsPerSecond, options.SyncOverAsync);

            writeResultsTask.Wait();

            return 0;
        }

        private static void RunTest(Uri uri, int requestsPerSecond, bool syncOverAsync)
        {
            while (true)
            {
                if (Interlocked.Read(ref _requests) < requestsPerSecond * _stopwatch.Elapsed.TotalSeconds)
                {
                    _ = ExecuteRequest(uri, syncOverAsync);
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                }
            }
        }

        private static async Task ExecuteRequest(Uri uri, bool syncOverAsync)
        {
            Interlocked.Increment(ref _requests);

            var start = _stopwatch.ElapsedTicks;
            var task = _httpClient.GetAsync(uri);
            if (syncOverAsync)
            {
                // Run synchronous Wait() on threadpool
                await Task.Yield();
                task.Wait();
            }
            else
            {
                await task;
            }
            var end = _stopwatch.ElapsedTicks;

            Interlocked.Add(ref _responseLatencyTicks, end - start);
            Interlocked.Increment(ref _responses);

            task.Result.Dispose();
        }

        private static async Task WriteResults()
        {
            var lastRequests = (long)0;
            var lastResponses = (long)0;
            var lastResponseLatencyTicks = (long)0;
            var lastElapsed = TimeSpan.Zero;

            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                var requests = Interlocked.Read(ref _requests);
                var currentRequests = requests - lastRequests;
                lastRequests = requests;

                var responses = Interlocked.Read(ref _responses);
                var currentResponses = responses - lastResponses;
                lastResponses = responses;

                var responseLatencyTicks = Interlocked.Read(ref _responseLatencyTicks);
                var currentResponseLatencyTicks = responseLatencyTicks - lastResponseLatencyTicks;
                lastResponseLatencyTicks = responseLatencyTicks;

                var elapsed = _stopwatch.Elapsed;
                var currentElapsed = elapsed - lastElapsed;
                lastElapsed = elapsed;

                WriteResult(requests, responses, responseLatencyTicks, elapsed, currentRequests, currentResponses, currentResponseLatencyTicks, currentElapsed);
            }
        }

        private static void WriteResult(long totalRequests, long totalResponses, long totalResponseLatencyTicks, TimeSpan totalElapsed,
            long currentRequests, long currentResponses, long currentResponseLatencyTicks, TimeSpan currentElapsed)
        {
            var totalResponseLatencyMs = ((double)totalResponseLatencyTicks / Stopwatch.Frequency) * 1000;
            var currentResponseLatencyMs = ((double)currentResponseLatencyTicks / Stopwatch.Frequency) * 1000;
            var threads = Process.GetCurrentProcess().Threads.Count;

            Console.WriteLine(
                $"{DateTime.UtcNow.ToString("o")}" +
                $"\tTot Req\t{totalRequests}" +
                $"\tTot Rsp\t{totalResponses}" +
                $"\tOut Req\t{totalRequests - totalResponses}" +
                $"\tCur Q/S\t{Math.Round(currentRequests / currentElapsed.TotalSeconds)}" +
                $"\tCur R/S\t{Math.Round(currentResponses / currentElapsed.TotalSeconds)}" +
                $"\tCur Lat\t{Math.Round(currentResponseLatencyMs / currentResponses, 0)}ms" +
                $"\tThreads\t{threads}"
                //$"\tAvg Q/S\t{Math.Round(totalRequests / totalElapsed.TotalSeconds)}" +
                //$"\tAvg R/S\t{Math.Round(totalResponses / totalElapsed.TotalSeconds)}" +
                //$"\tAvg Lat\t{Math.Round(totalResponseLatencyMs / totalRequests, 2)}ms"
            );
        }
    }
}
