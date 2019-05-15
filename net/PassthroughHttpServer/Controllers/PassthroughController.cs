using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PassthroughHttpServer.Controllers
{
    [Route("")]
    [ApiController]
    public class PassthroughController : ControllerBase
    {
        private static readonly HttpClient _httpClient = new HttpClient();
#if NET48
        private static readonly WebClient _webClient = new WebClient();
#endif

        [HttpGet]
        public async Task<ActionResult<string>> Get(string uri, string threadingModel = "async")
        {
            if (string.Equals(threadingModel, "async", StringComparison.OrdinalIgnoreCase))
            {
                var result = await _httpClient.GetAsync(uri);
                var content = await result.Content.ReadAsStringAsync();
                return content;
            }
            else if (string.Equals(threadingModel, "syncoverasync", StringComparison.OrdinalIgnoreCase))
            {
                var result = _httpClient.GetAsync(uri).Result;
                var content = result.Content.ReadAsStringAsync().Result;
                return content;
            }
            else if (string.Equals(threadingModel, "sync", StringComparison.OrdinalIgnoreCase))
            {
#if NET48
                return _webClient.DownloadString(uri);
#else
                throw new InvalidOperationException();
#endif
            }
            else
                    {
                throw new InvalidOperationException();
            }
        }
    }
}
