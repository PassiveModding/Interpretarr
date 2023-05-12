using Interpretarr.Clients.Sonarr;
using Interpretarr.Clients.Sonarr.Model.Episode;

namespace Interpretarr.Services
{
    public class IndyCarDataProvider
    {
        public IndyCarDataProvider(SonarrClient sonarrClient, ILogger<IndyCarDataProvider> logger)
        {
            this.sonarrClient = sonarrClient;
            this.logger = logger;
        }
        
        private const int tvdbId = 362670;
        private readonly SonarrClient sonarrClient;
        private readonly ILogger<IndyCarDataProvider> logger;
        private EpisodeResource[]? episodes = null;
        private DateTime lastUpdated = DateTime.MinValue;
        private SemaphoreSlim semaphore = new(1, 1);

        public async Task<EpisodeResource[]> GetAsync()
        {
            try
            {
                await semaphore.WaitAsync();

                // Update every 6 hours
                if (episodes == null || DateTime.UtcNow - lastUpdated > TimeSpan.FromHours(6))
                {            
                    logger.LogInformation("Updating data");
                    var series = await sonarrClient.GetSeriesAsync(tvdbId);
                    if (series.Length == 0)
                    {
                        logger.LogError("Series not found");
                        throw new Exception("Series not found");
                    }

                    episodes = await sonarrClient.GetEpisodesAsync(series[0].Id);
                    episodes = episodes.Where(x => x.SeasonNumber >= 2022).ToArray();

                    logger.LogInformation($"Data updated with {episodes.Length} episodes");
                }

                return episodes;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}