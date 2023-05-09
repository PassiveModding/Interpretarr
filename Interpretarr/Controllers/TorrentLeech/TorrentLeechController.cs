using System.Reflection.Metadata;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using Interpretarr.Model;
using Interpretarr.Clients.FlareSolver.Model;
using Interpretarr.Clients.FlareSolver;
using System.IO;
using System.Net.Http.Headers;
using System.Text;

namespace Interpretarr.Controllers
{

    [ApiController]
    [Route("/torrentleech/")]
    public class TorrentLeechController : ControllerBase
    {
        private readonly HttpClient httpClient;
        private readonly IEnumerable<IMiddlemanHelper> middlemanHelpers;
        private static Guid sessionId = Guid.NewGuid();
        private static bool sessionCreated = false;
        private const string websiteUrl = "https://www.torrentleech.org";

        // not a great workaround but for some reason prowlarr isn't saving the cookie so we save it here instead
        private static string? Cookie = null;

        public TorrentLeechController(HttpClient httpClient, IEnumerable<IMiddlemanHelper> middlemanHelpers)
        {
            this.httpClient = httpClient;
            this.middlemanHelpers = middlemanHelpers.Where(x => x.SupportedSite == SupportedSite.TorrentLeech);
        }

        [HttpPost("user/account/login")]
        public async Task<IActionResult> LoginAsync([FromForm] string username, [FromForm] string password, [FromForm] string? otpkey = null)
        {
            var httpClient = new HttpClient();

            // First request
            httpClient.DefaultRequestHeaders.Add("authority", "www.torrentleech.org");
            httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36");

            var loginUrl = "https://www.torrentleech.org/user/account/login/";

            HttpResponseMessage firstResponse = await httpClient.GetAsync(loginUrl);
            var firstResponseCookies = firstResponse.Headers.GetValues("set-cookie");

            // Second request
            httpClient.DefaultRequestHeaders.Add("cache-control", "max-age=0");
            httpClient.DefaultRequestHeaders.Add("cookie", string.Join("; ", firstResponseCookies));
            httpClient.DefaultRequestHeaders.Add("origin", "https://www.torrentleech.org");
            httpClient.DefaultRequestHeaders.Add("referer", loginUrl);

            string postData = $"username={username}&password={password}";
            HttpContent content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpResponseMessage secondResponse = await httpClient.PostAsync(loginUrl, content);

            var secondResponseCookies = secondResponse.Headers.GetValues("set-cookie");

            // save cookie
            Cookie = secondResponseCookies.Last();

            return Ok();
        }        

        [HttpGet("{*path}")]
        public async Task<IActionResult> GetAsync(string path)
        {
            Console.WriteLine($"GET {path}");
            foreach (var middlemanHelper in middlemanHelpers)
            {
                if (middlemanHelper.HandleRequest(path, out var updatedPath))
                {
                    Console.WriteLine($"GET {path} handled by {middlemanHelper.GetType().Name} => {updatedPath}");
                    var url = $"{websiteUrl}{(updatedPath.StartsWith('/') ? updatedPath : '/' + updatedPath)}";

                    using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        if (Cookie != null) request.Headers.Add("Cookie", Cookie);
                        var response = await httpClient.SendAsync(request);

                        var responseContent = await response.Content.ReadAsStringAsync();
                        var parsedResponse = middlemanHelper.HandleResponse(url, responseContent);

                        // return parsed response
                        return Ok(parsedResponse);
                    }
                }
            }

            using (var request = new HttpRequestMessage(HttpMethod.Get, $"{websiteUrl}{(path.StartsWith('/') ? path : '/' + path)}"))
            {
                if (Cookie != null) request.Headers.Add("Cookie", Cookie);
                var response = await httpClient.SendAsync(request);

                // if json return ok stringcontent otherwise return response
                if (response.Content.Headers?.ContentType?.MediaType == "application/json")
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return Ok(responseContent);
                }

                return new HttpResponseMessageResult(response);
            }  
        }
    }
}
