using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

#if NET48
using System.Net;
#else
using System.Net.Http;
#endif

namespace PassthroughHttpServer.Controllers
{
    [Route("")]
    [ApiController]
    public class PassthroughController : ControllerBase
    {
        private static int _minWorkerThreads;

#if !NET48
        private static readonly HttpClient _httpClient = new HttpClient();
#endif

        static PassthroughController()
        {
            ThreadPool.GetMinThreads(out _minWorkerThreads, out _);
        }

        [HttpGet]
        public async Task<ActionResult<string>> Get(string uri, string threadingModel = "async", int minWorkerThreads = -1)
        {
            try
            {
                Interlocked.Increment(ref Program.CurrentRequests);

                if (minWorkerThreads > -1 && minWorkerThreads != _minWorkerThreads)
                {
                    var previousMinWorkerThreads = Interlocked.Exchange(ref _minWorkerThreads, minWorkerThreads);
                    if (previousMinWorkerThreads != minWorkerThreads)
                    {
                        LogMinThreads();
                        ThreadPool.GetMinThreads(out _, out var minCompletionPortThreads);
                        ThreadPool.SetMinThreads(minWorkerThreads, minCompletionPortThreads);
                        LogMinThreads();
                        Console.WriteLine();
                    }
                }

                string content;

                if (string.Equals(threadingModel, "async", StringComparison.OrdinalIgnoreCase))
                {
#if NET48
                    using (var webClient = new WebClient())
                    {
                        content = await webClient.DownloadStringTaskAsync(uri);
                    }
#else
                    using (var response = await _httpClient.GetAsync(uri))
                    {
                        content = await response.Content.ReadAsStringAsync();
                    }
#endif
                }
                else if (string.Equals(threadingModel, "syncoverasync", StringComparison.OrdinalIgnoreCase))
                {
#if NET48
                    using (var webClient = new WebClient())
                    {
                        content = webClient.DownloadStringTaskAsync(uri).Result;
                    }
#else
                    using (var response = _httpClient.GetAsync(uri).Result)
                    {
                        content = response.Content.ReadAsStringAsync().Result;
                    }
#endif
                }
                else if (string.Equals(threadingModel, "sync", StringComparison.OrdinalIgnoreCase))
                {
#if NET48
                    using (var webClient = new WebClient())
                    {
                        content = webClient.DownloadString(uri);
                    }
#else
                    throw new InvalidOperationException("There is currently no fully sync HTTP client in .NET Core");
#endif
                }
                else
                {
                    throw new InvalidOperationException($"Invalid threadingModel: '{threadingModel}'");
                }

                Response.Headers.Add("Threads", Program.Threads.ToString());

                return content;
            }
            finally
            {
                Interlocked.Decrement(ref Program.CurrentRequests);
            }
        }

        private static void LogMinThreads()
        {
            ThreadPool.GetMinThreads(out var minWorkerThreads, out var minCompletionPortThreads);
            Console.WriteLine($"ThreadPool.GetMinThreads(): {minWorkerThreads}, {minCompletionPortThreads}");
        }
    }
}
