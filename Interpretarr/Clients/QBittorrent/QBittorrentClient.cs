using Interpretarr.Clients.QBittorrent.Model;
using Interpretarr.Config;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;

namespace Interpretarr.Clients.QBittorrent
{
    public class QBittorrentClient : IDisposable
    {
        public QBittorrentClient(IOptions<QBittorrentConfig> config)
        {
            _config = config.Value;
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_config.Host);
            HttpClient = httpClient;
        }

        private readonly QBittorrentConfig _config;
        public HttpClient HttpClient { get; }

        public async Task LoginAsync()
        {
            // curl -i --header 'Referer: http://localhost:8080' --data 'username={username}&password={password}' http://localhost:8080/api/v2/auth/login
            var url = $"/api/v2/auth/login";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Referer", _config.Host);
            request.Content = new StringContent($"username={_config.Username}&password={_config.Password}");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

            var response = await HttpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                var cookie = cookies.First();
                HttpClient.DefaultRequestHeaders.Add("Cookie", cookie);
            }
            else
            {
                throw new Exception("Failed to get cookie");
            }
        }

        public async Task<List<TorrentInfoResource>> GetTorrentsAsync()
        {
            var url = $"/api/v2/torrents/info";
            string? response = null;
            try
            {
                response = await HttpClient.GetStringAsync(url);
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    await LoginAsync();
                    response = await HttpClient.GetStringAsync(url);
                }
            }
            return JsonConvert.DeserializeObject<List<TorrentInfoResource>>(response) ?? throw new Exception();
        }



        public async Task<List<TorrentFileResource>> GetTorrentFilesAsync(string hash)
        {
            var url = $"/api/v2/torrents/files?hash={hash}";

            string? response = null;
            try
            {
                response = await HttpClient.GetStringAsync(url);
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    await LoginAsync();
                    response = await HttpClient.GetStringAsync(url);
                }
            }

            return JsonConvert.DeserializeObject<List<TorrentFileResource>>(response) ?? throw new Exception();
        }

        public async Task RenameFileAsync(string hash, string oldName, string newName)
        {
            var url = $"/api/v2/torrents/renameFile";
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("hash", hash),
                new KeyValuePair<string, string>("oldPath", oldName),
                new KeyValuePair<string, string>("newPath", newName)
            });

            try
            {
                await HttpClient.PostAsync(url, content);
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    await LoginAsync();
                    await HttpClient.PostAsync(url, content);
                }
            }
        }

        public async Task RenameTorrentAsync(string hash, string newName)
        {
            var url = $"/api/v2/torrents/rename";
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("hash", hash),
                new KeyValuePair<string, string>("name", newName)
            });

            try
            {
                await HttpClient.PostAsync(url, content);
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    await LoginAsync();
                    await HttpClient.PostAsync(url, content);
                }
            }
        }

        public void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}