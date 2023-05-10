// f1data.cs

using System.Text.RegularExpressions;
using Interpretarr.Clients.Sonarr;
using Interpretarr.Model;

namespace Interpretarr.Services
{
    public class F1DataProvider
    {
        public F1DataProvider(SonarrClient sonarrClient, ILogger<F1DataProvider> logger)
        {
            this.sonarrClient = sonarrClient;
            this.logger = logger;
        }
        
        private const int tvdbId = 387219;
        private readonly SonarrClient sonarrClient;
        private readonly ILogger<F1DataProvider> logger;
        private DateTime lastUpdated = DateTime.MinValue;
        private SemaphoreSlim semaphore = new(1, 1);

        private F1Data? f1Data = null;

        // Matches "Location (Session)" naming format for all releases 2014-present        
        public readonly Regex TitleRegex = new(@"^(.+) \((.+)\)$", RegexOptions.Compiled);

        public async Task<F1Data> GetAsync()
        {
            try
            {
                await semaphore.WaitAsync();

                // Update every 6 hours
                if (f1Data == null || DateTime.UtcNow - lastUpdated > TimeSpan.FromHours(6))
                {            
                    logger.LogInformation("Updating F1 data");
                    var series = await sonarrClient.GetSeriesAsync(tvdbId);
                    if (series.Length == 0)
                    {
                        logger.LogError("Series not found");
                        throw new Exception("Series not found");
                    }

                    var episodes = await sonarrClient.GetEpisodesAsync(series[0].Id);
                    // Filter out old seasons which do not follow the new naming scheme
                    episodes = episodes.Where(x => x.SeasonNumber >= 2014).ToArray();
                    var locations = new HashSet<string>();
                    var sessions = new HashSet<string>();
                    foreach (var episode in episodes)
                    {
                        var match = TitleRegex.Match(episode.Title);
                        if (match.Success)
                        {
                            var location = match.Groups[1].Value;
                            var session = match.Groups[2].Value;
                            locations.Add(location);
                            sessions.Add(session);
                        }
                    }

                    // remove locations with numbers in them to remove things like 70th Anniversary, 1st test etc.
                    locations = locations.Where(x => !Regex.IsMatch(x, @"\d")).ToHashSet();

                    lastUpdated = DateTime.UtcNow;
                    f1Data = new F1Data(episodes, locations, sessions);
                    logger.LogInformation($"F1 data updated with {locations.Count} locations and {sessions.Count} sessions");
                }

                return f1Data;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}