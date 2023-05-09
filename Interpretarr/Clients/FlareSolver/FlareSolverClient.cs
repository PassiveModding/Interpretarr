using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Interpretarr.Clients.FlareSolver.Model;
using Interpretarr.Config;
using Interpretarr.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Interpretarr.Clients.FlareSolver
{
    public class FlareSolverClient : IDisposable
    {
        public readonly FlareSolverConfig Config;
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);
        public FlareSolverClient(IOptions<FlareSolverConfig> config)
        {
            Config = config.Value;
            HttpClient = new HttpClient();
        }

        public HttpClient HttpClient { get; }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, Config.Host + "/v1");
                request.Content = JsonContent.Create(new
                {
                    cmd = "request.get",
                    url,
                    wait = 0,
                    maxTimeout = 60000
                });

                var fsResponse = await HttpClient.SendAsync(request);
                return fsResponse;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<FlareSolverResponse> ParseResponseAsync(HttpResponseMessage flaresolverResponse)
        {
            var parsedResponse = JsonConvert.DeserializeObject<FlareSolverResponse>(await flaresolverResponse.Content.ReadAsStringAsync());
            return parsedResponse;
        }

        // Rewrites the response to match the original request
        // TODO: This is a mess, needs to be cleaned up or removed outright. I don't like being dependent on flaresolverr
        public async Task<HttpResponseMessageResult> ParseResponseAsync(HttpResponseMessage flaresolverResponse, (string url, IMiddlemanHelper middleman)? middlemanHelper = null)
        {
            try
            {
                var parsedResponse = JsonConvert.DeserializeObject<FlareSolverResponse>(await flaresolverResponse.Content.ReadAsStringAsync());
                var responseBody = parsedResponse?.Solution?.Response ?? string.Empty;
                if (middlemanHelper.HasValue)
                {
                    responseBody = middlemanHelper.Value.middleman.HandleResponse(middlemanHelper.Value.url, responseBody);
                }

                var response = new HttpResponseMessage
                {
                    Content = new StringContent(responseBody),
                    StatusCode = Enum.Parse<HttpStatusCode>(parsedResponse?.Solution?.Status ?? parsedResponse?.Status ?? "500"),
                };
                // try to set contenttype based on response
                if (responseBody.StartsWith("<!DOCTYPE html>") || responseBody.StartsWith("<html"))
                {
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                }
                else if (responseBody.StartsWith("{") || responseBody.StartsWith("["))
                {
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                }
                else if (responseBody.StartsWith("<?xml"))
                {
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
                }
                else if (responseBody.StartsWith("<"))
                {
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
                }
                else
                {
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                }

                if ((parsedResponse?.Solution?.Cookies) != null)
                {
                    foreach (var cookie in parsedResponse.Solution.Cookies)
                    {
                        response.Headers.Add("Set-Cookie", $"{cookie.Name}={cookie.Value}; Domain={cookie.Domain}; Path={cookie.Path}; Expires={cookie.Expires}; HttpOnly={cookie.HttpOnly}; Secure={cookie.Secure}");
                    }
                }

                return new HttpResponseMessageResult(response);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}
