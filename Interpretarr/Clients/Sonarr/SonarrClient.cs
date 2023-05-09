using Interpretarr.Clients.Sonarr.Model.Command;
using Interpretarr.Clients.Sonarr.Model.CommandPost;
using Interpretarr.Clients.Sonarr.Model.Episode;
using Interpretarr.Clients.Sonarr.Model.Queue;
using Interpretarr.Clients.Sonarr.Model.Series;
using Interpretarr.Config;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Interpretarr.Clients.Sonarr
{
    public class SonarrClient : IDisposable
    {
        private readonly SonarrConfig _config;

        public SonarrClient(IOptions<SonarrConfig> config)
        {
            _config = config.Value;
            var client = new HttpClient();
            //client.DefaultRequestHeaders.Add("X-Api-Key", config.ApiKey);
            client.BaseAddress = new Uri(_config.Host);
            HttpClient = client;
        }

        public HttpClient HttpClient { get; }

        public async Task<CommandResource> CommandAsync(CommandPostResource commandPostResource)
        {
            var response = await HttpClient.PostAsync($"/api/v3/command?apikey={_config.ApiKey}", new StringContent(JsonConvert.SerializeObject(commandPostResource)));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var command = JsonConvert.DeserializeObject<CommandResource>(content);
            return command ?? throw new Exception("No command response");
        }

        public async Task<SeriesResource[]> GetSeriesAsync(int? tvdbId)
        {
            var response = await HttpClient.GetAsync($"/api/v3/series?apikey={_config.ApiKey}&tvdbId={tvdbId}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var series = JsonConvert.DeserializeObject<SeriesResource[]>(content);
            return series ?? throw new Exception("No series response");
        }

        public async Task<EpisodeResource[]> GetEpisodesAsync(long seriesId)
        {
            var response = await HttpClient.GetAsync($"/api/v3/episode?apikey={_config.ApiKey}&seriesId={seriesId}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var episodes = JsonConvert.DeserializeObject<EpisodeResource[]>(content);
            return episodes ?? throw new Exception("No episodes response");
        }

        public async Task<QueueResource[]> GetQueueAsync()
        {
            var response = await HttpClient.GetAsync($"/api/v3/queue/details?apikey={_config.ApiKey}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var queue = JsonConvert.DeserializeObject<QueueResource[]>(content);
            return queue ?? throw new Exception("No queue response");
        }

        public async Task RemoveFromQueueAsync(long queueItem)
        {
            var response = await HttpClient.DeleteAsync($"/api/v3/queue/{queueItem}?apikey={_config.ApiKey}&removeFromClient=false&blocklist=false");
            response.EnsureSuccessStatusCode();
        }


        public void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}
