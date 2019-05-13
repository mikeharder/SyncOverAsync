using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Net;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace DelayHttpServer
{
    class Program
    {
        private static readonly byte[] _helloWorldPayload = Encoding.UTF8.GetBytes("Hello, World!");
        private static readonly int _helloWorldPayloadLength = _helloWorldPayload.Length;

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

            Console.WriteLine();

            new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, 8080);
                })
                .Configure(app => app.Run(async context =>
                {
                    var delayMilliseconds = context.Request.Path.Value.Substring(1).Trim(new char[] { '/' });

                    if (!string.IsNullOrEmpty(delayMilliseconds))
                    {
                        // Simulate server processing time
                        await Task.Delay(int.Parse(delayMilliseconds));
                    }

                    var response = context.Response;
                    response.StatusCode = 200;
                    response.ContentType = "text/plain";
                    response.ContentLength = _helloWorldPayloadLength;
                    await response.Body.WriteAsync(_helloWorldPayload, 0, _helloWorldPayloadLength);
                }))
                .Build()
                .Run();
        }
    }
}
