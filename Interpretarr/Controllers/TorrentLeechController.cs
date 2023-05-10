using Microsoft.AspNetCore.Mvc;
using Interpretarr.Model;
using System.Text;
using Newtonsoft.Json;
using Interpretarr.Clients.TorrentLeech.Model;
using System.Text.RegularExpressions;

namespace Interpretarr.Controllers
{

    [ApiController]
    [Route("/torrentleech/")]
    public class TorrentLeechController : MiddlemanControllerBase
    {
        // not a great workaround but for some reason prowlarr isn't saving the cookie so we save it here instead
        private static string? Cookie = null;

        public TorrentLeechController(HttpClient httpClient, IEnumerable<IMiddlemanHelper> middlemanHelpers, ILogger<TorrentLeechController> logger) : 
            base(httpClient, middlemanHelpers, SupportedSite.TorrentLeech, "https://www.torrentleech.org", logger)
        {
        }

        [HttpPost("user/account/login")]
        public async Task<IActionResult> LoginAsync([FromForm] string username, [FromForm] string password, [FromForm] string? otpkey = null)
        {
            var loginUrl = $"{websiteUrl}/user/account/login/";

            IEnumerable<string> firstResponseCookies;
            using (var firstRequest = new HttpRequestMessage(HttpMethod.Get, loginUrl))
            {
                firstRequest.Headers.Add("authority", "www.torrentleech.org");
                firstRequest.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36");

                var firstResponse = await httpClient.SendAsync(firstRequest);

                if (firstResponse.Headers.Contains("set-cookie"))
                {
                    firstResponseCookies = firstResponse.Headers.GetValues("set-cookie");
                }
                else 
                {
                    firstResponseCookies = new List<string>();
                }
            }

            using (var secondRequest = new HttpRequestMessage(HttpMethod.Post, loginUrl))
            {
                secondRequest.Headers.Add("authority", "www.torrentleech.org");
                secondRequest.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36");
                secondRequest.Headers.Add("origin", websiteUrl);
                secondRequest.Headers.Add("referer", loginUrl);
                secondRequest.Headers.Add("cookie", string.Join("; ", firstResponseCookies));

                string postData = $"username={username}&password={password}";
                if (otpkey != null)
                {
                    postData += $"&otpkey={otpkey}";
                }

                secondRequest.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var secondResponse = await httpClient.SendAsync(secondRequest);
                if (secondRequest.Headers.Contains("set-cookie"))
                {
                    var secondResponseCookies = secondResponse.Headers.GetValues("set-cookie");

                    // save cookie
                    Cookie = secondResponseCookies.Last();
                }

                return Ok();
            }
        }

        public override bool GetKeywords(string path, out string keywords)
        {
            keywords = null!;
            if (!path.StartsWith("torrents/browse/list/categories/", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var match = Regex.Match(path, @"query/(.*?)/");
            if (match.Success)
            {
                keywords = match.Groups[1].Value;
                return true;
            }

            return false;
        }

        public override bool RewritePath(string path, string updatedKeywords, out string updatedPath)
        {
            updatedPath = null!;
            if (!path.StartsWith("torrents/browse/list/categories/", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var match = Regex.Match(path, @"query/(.*?)/");
            if (match.Success)
            {
                updatedPath = path.Replace(match.Groups[0].Value, $"query/{updatedKeywords}/");
                return true;
            }

            return false;
        }
        
        public override async Task<FormatResult> FormatResponseAsync(string body, IMiddlemanHelper middlemanHelper)
        {
            var torrentLeechResponse = JsonConvert.DeserializeObject<Query>(body);
            if (torrentLeechResponse == null)
            {
                return new FormatResult(false, body);
            }

            foreach (var torrent in torrentLeechResponse.TorrentList)
            {
                var titleResult = await middlemanHelper.FormatReleaseTitleAsync(torrent.Name);
                if (titleResult.Success == false || titleResult.Result == null) continue;
                torrent.Name = titleResult.Result;
            }

            // serialize but keep any fields that were excluded in serializing to query since body typically contains extra fields that mey be required
            var bodyObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(body);
            bodyObject["torrentList"] = torrentLeechResponse.TorrentList;
            return new FormatResult(true, JsonConvert.SerializeObject(bodyObject));
        }

        public override void SetupRequest(HttpRequestMessage request)
        {            
            if (Cookie != null) request.Headers.Add("Cookie", Cookie);
        }
    }
}
