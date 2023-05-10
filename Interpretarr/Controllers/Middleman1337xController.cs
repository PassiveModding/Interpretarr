using Microsoft.AspNetCore.Mvc;
using Interpretarr.Model;
using HtmlAgilityPack;

namespace Interpretarr.Controllers
{

    [ApiController]
    [Route("/1337xMiddleman/")]
    public class Middleman1337xController : MiddlemanControllerBase
    {
        public Middleman1337xController(HttpClient httpClient, IEnumerable<IMiddlemanHelper> middlemanHelpers, ILogger<Middleman1337xController> logger) : 
            base(httpClient, middlemanHelpers, SupportedSite._1337xTo, "https://1337x.to", logger)
        {
        }

        public override async Task<FormatResult> FormatResponseAsync(string body, IMiddlemanHelper middlemanHelper)
        {
            // modify body to use the parsed release title instead of the original
            var doc = new HtmlDocument();
            doc.LoadHtml(body);

            var torrentList = doc.DocumentNode.SelectNodes("//a[contains(@href, '/torrent/')]");
            if (torrentList == null)
            {
                return new FormatResult(false, body);
            }

            try
            {
                // iterate over the torrent list and parse the release title
                bool success = false;
                foreach (var torrent in torrentList)
                {
                    var releaseTitle = torrent.InnerText;
                    var titleResult = await middlemanHelper.FormatReleaseTitleAsync(releaseTitle);

                    if (titleResult.Success)
                    {
                        logger.LogInformation($"Parsed {releaseTitle} to {titleResult.Result}");
                        torrent.InnerHtml = titleResult.Result;
                        success = true;
                    }
                }

                return new FormatResult(success, doc.DocumentNode.OuterHtml);
            }
            catch (Exception)
            {
                return new FormatResult(false, body);
            }
        }

        public override void SetupRequest(HttpRequestMessage request)
        {
            //
        }

        public override bool GetKeywords(string path, out string keywords)
        {
            keywords = null!;
            if (!path.StartsWith("sort-search", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // parse out the search term
            var pathSections = path.Split('/');
            if (pathSections.Length < 2)
            {
                return false;
            }

            keywords = pathSections[1];
            return true;
        }

        public override bool RewritePath(string path, string updatedKeywords, out string updatedPath)
        {
            updatedPath = null!;
            if (!path.StartsWith("sort-search", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // parse out the search term
            var pathSections = path.Split('/');
            if (pathSections.Length < 2)
            {
                return false;
            }

            pathSections[1] = updatedKeywords;
            updatedPath = string.Join('/', pathSections);
            return true;
        }
    }
}
