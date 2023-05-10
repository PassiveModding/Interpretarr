using Interpretarr.Model;
using Microsoft.AspNetCore.Mvc;
namespace Interpretarr.Controllers
{
    public abstract class MiddlemanControllerBase : ControllerBase
    {
        protected readonly IEnumerable<IMiddlemanHelper> middlemanHelpers;
        protected readonly HttpClient httpClient;
        protected readonly SupportedSite supportedSite;
        protected readonly string websiteUrl;
        protected readonly ILogger logger;

        public MiddlemanControllerBase(HttpClient httpClient, IEnumerable<IMiddlemanHelper> middlemanHelpers, SupportedSite supportedSite, string url, ILogger logger)
        {
            this.middlemanHelpers = middlemanHelpers.Where(x => x.SupportedSites.Contains(supportedSite));
            this.httpClient = httpClient;
            this.supportedSite = supportedSite;
            this.websiteUrl = url;
            this.logger = logger;
        }


        public abstract Task<FormatResult> FormatResponseAsync(string body, IMiddlemanHelper middlemanHelper);

        public abstract void SetupRequest(HttpRequestMessage request);

        public abstract bool GetKeywords(string path, out string keywords); 
        public abstract bool RewritePath(string path, string updatedKeywords, out string updatedPath);

        public async Task<IActionResult> HandleAsync(string path)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"{websiteUrl}/{path}"))
            {
                SetupRequest(request);
                var response = await httpClient.SendAsync(request);
                return new ContentResult 
                {
                    Content = await response.Content.ReadAsStringAsync(),
                    ContentType = response.Content.Headers?.ContentType?.MediaType,
                    StatusCode = (int)response.StatusCode
                };
            }  
        }

        [HttpGet("{*path}")]
        public async Task<IActionResult> GetAsync(string path)
        {
            // Get keywords from path
            if (!GetKeywords(path, out var keywords))
            {
                return await HandleAsync(path);
            }

            // Iterate middlemanHelpers
            foreach (var helper in middlemanHelpers)
            {
                // If middlemanHelper can handle keywords, update path and send request
                // If middlemanHelper can't handle keywords, continue
                var keywordResponse = await helper.FormatKeywordsAsync(keywords);
                if (keywordResponse.Success == false || keywordResponse.Result == null) continue;

                var updatedKeywords = keywordResponse.Result;

                // Rewrite path with updated keywords
                if (!RewritePath(path, updatedKeywords, out var updatedPath)) continue; 

                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{websiteUrl}/{updatedPath}"))
                {
                    SetupRequest(request);
                    var response = await httpClient.SendAsync(request);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        return GetContentResult(responseContent, response);
                    }

                    var updatedContent = await FormatResponseAsync(responseContent, helper);
                    if (updatedContent.Success == false || updatedContent.Result == null) continue;
                    return GetContentResult(updatedContent.Result, response);
                }
            }

            // If no middlemanHelper can handle keywords, send request
            return await HandleAsync(path);
        }    
        
        public ContentResult GetContentResult(string content, HttpResponseMessage response)
        {
            return new ContentResult
            {
                Content = content,
                ContentType = response.Content.Headers?.ContentType?.MediaType,
                StatusCode = (int)response.StatusCode
            };
        }
    }
}