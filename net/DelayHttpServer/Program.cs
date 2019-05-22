using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Diagnostics;
using System.Net;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DelayHttpServer
{
    class Program
    {
        private static int _currentRequests;

        private static readonly char[] _pathTrimChars = new char[] { '/' };
        private static readonly byte[] _helloWorldPayload = Encoding.UTF8.GetBytes("Hello, World!");
        private static readonly int _helloWorldPayloadLength = _helloWorldPayload.Length;

        private static int _additionalDelay = 0;

        static void Main(string[] args)
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

            Console.WriteLine($"AdditionalDelay: {_additionalDelay}");

            Console.WriteLine();

            ThreadPool.QueueUserWorkItem(state => WriteResults());

            new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, 8080);
                })
                .Configure(app => app.Run(async context =>
                {
                    var path = context.Request.Path.Value;
                    var trimmedPath = path.Trim(_pathTrimChars);

                    if (path.Contains("delay", StringComparison.OrdinalIgnoreCase)) {
                        // trimmedPath: "delay/2000"
                        _additionalDelay = int.Parse(path.Trim(_pathTrimChars).Substring(path.LastIndexOf('/')));
                        Console.WriteLine($"AdditionalDelay: {_additionalDelay}");
                        Console.WriteLine();
                    }
                    else
                    {
                        // trimmedPath: "1000"
                        try
                        {
                            Interlocked.Increment(ref _currentRequests);

                            // Calculate total delay
                            var delay = _additionalDelay;
                            if (!string.IsNullOrEmpty(trimmedPath))
                            {
                                delay += int.Parse(trimmedPath);
                            }

                            // Simulate server processing time
                            if (delay > 0)
                            {
                                await Task.Delay(delay);
                            }

                            var response = context.Response;
                            response.StatusCode = 200;
                            response.ContentType = "text/plain";
                            response.ContentLength = _helloWorldPayloadLength;

                            await response.Body.WriteAsync(_helloWorldPayload, 0, _helloWorldPayloadLength);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _currentRequests);
                        }
                    }
                }))
                .Build()
                .Run();
        }

        private static void WriteResults()
        {
            var lastUserProcessorTime = TimeSpan.Zero;
            var lastPrivilegedProcessorTime = TimeSpan.Zero;
            var lastTotalProcessorTime = TimeSpan.Zero;
            var lastElapsed = TimeSpan.Zero;
            var stopwatch = Stopwatch.StartNew();

            while (true)
            {
                Thread.Sleep(1000);

                var process = Process.GetCurrentProcess();

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

                WriteResult(currentUserProcessorTime, currentPrivilegedProcessorTime, currentTotalProcessorTime, currentElapsed);
            }
        }

        private static void WriteResult(TimeSpan currentUserProcessorTime, TimeSpan currentPrivilegedProcessorTime,
            TimeSpan currentTotalProcessorTime, TimeSpan currentElapsed)
        {
            var currentElapsedCpuTicks = currentElapsed.Ticks * Environment.ProcessorCount;
            var currentUserProcessorPercentage = ((double)currentUserProcessorTime.Ticks) / currentElapsedCpuTicks;
            var currentPrivilegedProcessorPercentage = ((double)currentPrivilegedProcessorTime.Ticks) / currentElapsedCpuTicks;
            var currentTotalProcessorPercentage = ((double)currentTotalProcessorTime.Ticks) / currentElapsedCpuTicks;

            var threads = Process.GetCurrentProcess().Threads.Count;

            Console.WriteLine(
                $"{DateTime.UtcNow.ToString("o")}" +
                $"\tCur Req\t{_currentRequests}" +
                $"\tThreads\t{threads}" +
                $"\tUsr CPU\t{currentUserProcessorPercentage:P1}" +
                $"\tPrv CPU\t{currentPrivilegedProcessorPercentage:P1}" +
                $"\tTot CPU\t{currentTotalProcessorPercentage:P1}"
            );
        }
    }
}
