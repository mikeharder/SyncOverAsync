using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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
#if !NET48
        private static readonly HttpClient _httpClient = new HttpClient();
#endif

        [HttpGet]
        public async Task<ActionResult<string>> Get(string uri, string threadingModel = "async")
        {
            Response.Headers.Add("Threads", Process.GetCurrentProcess().Threads.Count.ToString());

            string content;

            if (string.Equals(threadingModel, "async", StringComparison.OrdinalIgnoreCase))
            {
#if NET48
                using (var webClient = new WebClient()) {
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

            return content;
        }
    }
}
