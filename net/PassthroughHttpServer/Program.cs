using System;
using System.Net;
using System.Runtime;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace PassthroughHttpServer
{
    public class Program
    {
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

            Console.WriteLine();

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
