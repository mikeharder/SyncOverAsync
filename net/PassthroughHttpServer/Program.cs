﻿using System;
using System.Diagnostics;
using System.Net;
using System.Runtime;
using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace PassthroughHttpServer
{
    public class Program
    {
        public static int CurrentRequests;

        public static void Main(string[] args)
        {
#if RELEASE
            Console.WriteLine("Configuration: Release");
#else
#error Must be built in 'Release' configuration
#endif

            if (GCSettings.IsServerGC)
            {
                Console.WriteLine("GC: Server");
            }
            else
            {
                throw new InvalidOperationException("Must be run with server GC");
            }

            ThreadPool.GetMinThreads(out var minWorkerThreads, out var minCompletionPortThreads);
            ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);
            Console.WriteLine($"ThreadPool.GetMinThreads(): {minWorkerThreads}, {minCompletionPortThreads}");
            Console.WriteLine($"ThreadPool.GetMaxThreads(): {maxWorkerThreads}, {maxCompletionPortThreads}");

            Console.WriteLine();

            ThreadPool.QueueUserWorkItem(state => WriteResults());

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://0.0.0.0:8081")
                .UseStartup<Startup>();

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
                $"\tCur Req\t{CurrentRequests}" +
                $"\tThreads\t{threads}" +
                $"\tUsr CPU\t{currentUserProcessorPercentage:P1}" +
                $"\tPrv CPU\t{currentPrivilegedProcessorPercentage:P1}" +
                $"\tTot CPU\t{currentTotalProcessorPercentage:P1}"
            );
        }
    }
}
