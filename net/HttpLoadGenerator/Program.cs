using CommandLine;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;

namespace HttpLoadGenerator
{
    class Program
    {
        private static HttpClient _httpClient = new HttpClient();
        private static Stopwatch _stopwatch;

        private static int _requests;
        private static int _successfulResponses;
        private static int _failedResponses;
        private static long _responseLatencyTicks;
        private static int _serverThreads;
        private static ConcurrentBag<ResponseData> _responseData;

        private static int Responses => _successfulResponses + _failedResponses;

        private class Options
        {
            [Option('u', "uri", Required = true)]
            public Uri Uri { get; set; }

            [Option('r', "requestsPerSecond", Default = 10)]
            public int RequestsPerSecond { get; set; }

            [Option('d', "durationSeconds", Default = 10)]
            public int DurationSeconds { get; set; }

            [Option('w', "warmupDurationSeconds", Default = 10, HelpText = "Warmup duration in seconds.  Set to 0 to skip warmup.")]
            public int WarmupDurationSeconds { get; set; }

            [Option('o', "outputFile", Default = "latency.csv")]
            public string OutputFile { get; set; }
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

            if (options.WarmupDurationSeconds > 0)
            {
                Console.WriteLine($"Warming up for {options.WarmupDurationSeconds} seconds...");
                RunTest(options.Uri, options.RequestsPerSecond, options.WarmupDurationSeconds);

                var warmupPath = Path.Combine(Path.GetDirectoryName(options.OutputFile), string.Concat(
                    Path.GetFileNameWithoutExtension(options.OutputFile),
                    ".warmup",
                    Path.GetExtension(options.OutputFile)));
                WriteLatency(warmupPath);

                Console.WriteLine();
            }

            Console.WriteLine($"Running for {options.DurationSeconds} seconds...");
            RunTest(options.Uri, options.RequestsPerSecond, options.DurationSeconds);

            WriteLatency(options.OutputFile);

            return 0;
        }

        private static void RunTest(Uri uri, int requestsPerSecond, int durationSeconds)
        {
            // Initialize data collection fields
            _requests = 0;
            _successfulResponses = 0;
            _failedResponses = 0;
            _responseLatencyTicks = 0;
            _responseData = new ConcurrentBag<ResponseData>();
            _stopwatch = Stopwatch.StartNew();

            var writeResultsThread = new Thread(() => WriteResults(durationSeconds));
            writeResultsThread.Start();

            while (_stopwatch.Elapsed.TotalSeconds < durationSeconds)
            {
                if (_requests < (requestsPerSecond * _stopwatch.Elapsed.TotalSeconds))
                {
                    _ = ExecuteRequest(uri);
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                }
            }

            writeResultsThread.Join();
        }

        private static async Task ExecuteRequest(Uri uri, bool warmup = false)
        {
            try
            {
                Interlocked.Increment(ref _requests);

                var start = _stopwatch.ElapsedTicks;
                using (var response = await _httpClient.GetAsync(uri))
                {
                    var end = _stopwatch.ElapsedTicks;

                    Interlocked.Add(ref _responseLatencyTicks, end - start);

                    if (response.IsSuccessStatusCode)
                    {
                        Interlocked.Increment(ref _successfulResponses);
                    }
                    else
                    {
                        Interlocked.Increment(ref _failedResponses);
                    }

                    // If value cannot be parsed, "threads" will be set to 0
                    int.TryParse(response.Headers.GetValues("Threads").SingleOrDefault(), out var threads);
                    _serverThreads = threads;

                    _responseData.Add(new ResponseData
                    {
                        StartTicks = start,
                        EndTicks = end,
                        Status = response.StatusCode,
                        ServerThreads = threads
                    });
                }
            }
            catch (Exception e)
            {
                // Nothing awaits the result of this method, so exceptions must be handled manually.
                Console.WriteLine(e);
            }
        }

        private static void WriteResults(int durationSeconds)
        {
            var lastRequests = 0;
            var lastSuccessfulResponses = 0;
            var lastFailedResponses = 0;
            var lastResponseLatencyTicks = (long)0;
            var lastElapsed = TimeSpan.Zero;

            while (_stopwatch.Elapsed.TotalSeconds < durationSeconds || Responses < _requests)
            {
                Thread.Sleep(1000);

                var requests = _requests;
                var currentRequests = requests - lastRequests;
                lastRequests = requests;

                var successfulResponses = _successfulResponses;
                var currentSuccessfulResponses = successfulResponses - lastSuccessfulResponses;
                lastSuccessfulResponses = successfulResponses;

                var failedResponses = _failedResponses;
                var currentFailedResponses = failedResponses - lastFailedResponses;
                lastFailedResponses = failedResponses;

                var responseLatencyTicks = Interlocked.Read(ref _responseLatencyTicks);
                var currentResponseLatencyTicks = responseLatencyTicks - lastResponseLatencyTicks;
                lastResponseLatencyTicks = responseLatencyTicks;

                var elapsed = _stopwatch.Elapsed;
                var currentElapsed = elapsed - lastElapsed;
                lastElapsed = elapsed;

                WriteResult(requests, successfulResponses, failedResponses,
                    currentRequests, currentSuccessfulResponses, currentFailedResponses, currentResponseLatencyTicks, currentElapsed);
            }
        }

        private static void WriteResult(int totalRequests, int totalSuccessfulResponses, int totalFailedResponses,
            int currentRequests, int currentSuccessfulResponses, int currentFailedResponses, long currentResponseLatencyTicks,
            TimeSpan currentElapsed)
        {
            var currentResponseLatencyMs = ((double)currentResponseLatencyTicks / Stopwatch.Frequency) * 1000;

            Console.WriteLine(
                $"{DateTime.UtcNow.ToString("o")}" +
                $"\tTot Req\t{totalRequests}" +
                $"\tOut Req\t{totalRequests - totalSuccessfulResponses - totalFailedResponses}" +
                $"\tSuc Rsp\t{totalSuccessfulResponses}" +
                $"\tErr Rsp\t{totalFailedResponses}" +
                $"\tCur Q/s\t{Math.Round(currentRequests / currentElapsed.TotalSeconds)}" +
                $"\tCur S/s\t{Math.Round(currentSuccessfulResponses / currentElapsed.TotalSeconds)}" +
                $"\tCur E/s\t{Math.Round(currentFailedResponses / currentElapsed.TotalSeconds)}" +
                $"\tCur Lat\t{Math.Round(currentResponseLatencyMs / (currentSuccessfulResponses + currentFailedResponses), 0)}ms" +
                $"\tSrv Thd\t{_serverThreads}"
            );
        }

        private static void WriteLatency(string path)
        {
            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("StartMs,LatencyMs,HttpStatus,ServerThreads");
                foreach (var responseData in _responseData.OrderBy(r => r.StartTicks))
                {
                    writer.Write(responseData.StartMilliseconds);
                    writer.Write(',');
                    writer.Write(responseData.LatencyMilliseconds);
                    writer.Write(',');
                    writer.Write((int)responseData.Status);
                    writer.Write(',');
                    writer.WriteLine(responseData.ServerThreads);
                }
            }
        }

        private class ResponseData
        {
            public long StartTicks { get; set; }
            public long EndTicks { get; set; }
            public HttpStatusCode Status { get; set; }
            public int ServerThreads { get; set; }

            public double StartMilliseconds => ((double)StartTicks / Stopwatch.Frequency) * 1000;
            public double LatencyMilliseconds => (((double)(EndTicks - StartTicks)) / Stopwatch.Frequency) * 1000;
        }
    }
}
