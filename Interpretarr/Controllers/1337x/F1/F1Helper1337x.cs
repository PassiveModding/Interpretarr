using FuzzySharp;
using HtmlAgilityPack;
using Interpretarr.Clients.Sonarr;
using Interpretarr.Clients.Sonarr.Model.Episode;
using Interpretarr.Model;
using System.Text.RegularExpressions;

namespace Interpretarr.Services.F1
{
    // Supports TVDB naming scheme 2014+ (last tested 2023)
    // Names:
    // Location (Practice 1)
    // Location (Practice 2)
    // Location (Practice 3)
    // Location (Qualifying)
    // Location (Sprint)
    // Location (Sprint Shootout)
    // Locaiton (Race)
    // NOTE: will not match testing episodes.
    public partial class F1Helper1337x : IMiddlemanHelper
    {
        private const int tvdbId = 387219;

        private EpisodeResource[] Episodes { get; set; }
        private HashSet<string> Locations { get; set; } = new();
        private HashSet<string> Sessions { get; set; } = new();

        public SupportedSite SupportedSite => SupportedSite.x1337xTo;

        public SonarrClient SonarrClient { get; }

        // Used to parse sonarr title from Bahrain (Practice 1)
        public readonly Regex TitleRegex = new(@"^(.+) \((.+)\)$", RegexOptions.Compiled);

        // Used to rewrite sonarr search from Formula 1 S2021E01 to Formula 1 2021 Bahrain
        public readonly Regex SonarrSearchRegex = new(@"^Formula\s+1\s+S(\d+)E(\d+)$", RegexOptions.Compiled);

        // More lenient regex since releases are not always consistent
        public readonly Regex F1AltReleaseRegex = new(@"^Formula\.1\.(\d{4})\.Round\.(\d+)\.(.+)$", RegexOptions.Compiled);        
        //public readonly Regex F1ReleaseRegex = new(@"^Formula\.1\.(\d{4})\.Round\.(\d+)\.(\w+)\.([\w]+)\.(\w+).(\w+)", RegexOptions.Compiled);

        public F1Helper1337x(SonarrClient sonarrClient)
        {
            SonarrClient = sonarrClient;
        }

        private void EnsureInitialized()
        {
            if (Episodes != null)
            {
                return;
            }

            var series = SonarrClient.GetSeriesAsync(tvdbId).GetAwaiter().GetResult();
            if (series.Length == 0)
            {
                throw new Exception("Series not found");
            }
            Episodes = SonarrClient.GetEpisodesAsync(series[0].Id).GetAwaiter().GetResult();
            // Filter out old seasons which do not follow the new naming scheme
            Episodes = Episodes.Where(x => x.SeasonNumber >= 2014).ToArray();

            foreach (var episode in Episodes)
            {
                var match = TitleRegex.Match(episode.Title);
                if (match.Success)
                {
                    var location = match.Groups[1].Value;
                    var session = match.Groups[2].Value;
                    Locations.Add(location);
                    Sessions.Add(session);
                }
            }
        }

        public string HandleResponse(string path, string body)
        {
            EnsureInitialized();
            // modify body to use the parsed release title instead of the original
            var doc = new HtmlDocument();
            doc.LoadHtml(body);

            var torrentList = doc.DocumentNode.SelectNodes("//a[contains(@href, '/torrent/')]");
            if (torrentList == null)
            {
                return body;
            }

            try
            {
                // iterate over the torrent list and parse the release title
                foreach (var torrent in torrentList)
                {
                    var releaseTitle = torrent.InnerText;
                    if (ParseReleaseTitle(releaseTitle, out var updatedTitle))
                    {
                        Console.WriteLine($"Parsed {releaseTitle} to {updatedTitle}");
                        torrent.InnerHtml = updatedTitle;
                    }
                }

                return doc.DocumentNode.OuterHtml;
            }
            catch (Exception)
            {
                return body;
            }
        }

        public bool HandleRequest(string path, out string updatedPath)
        {
            EnsureInitialized();
            updatedPath = path;
            // remove the leading slash if it exists, return it at the end.
            var initialPath = path;
            var leadingSlash = path.StartsWith('/');
            path = path.TrimStart('/');
            if (!path.StartsWith("sort-search", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // parse out the search term
            var searchTerm = path.Split('/')[1];
            var match = SonarrSearchRegex.Match(searchTerm);
            if (!match.Success)
            {
                return false;
            }

            var season = int.Parse(match.Groups[1].Value);
            var episode = int.Parse(match.Groups[2].Value);

            var episodeInfo = Episodes.FirstOrDefault(x => x.SeasonNumber == season && x.EpisodeNumber == episode);
            if (episodeInfo == null)
            {
                return false;
            }

            // take first word of title as location (ie abu for abu dhabi, united for united states, etc)
            // this narrows down the search results enough to find just the region.
            // the search results don't always have the full location or it is abbreviated differently. ie. Saudi Arabia vs. SaudiArabianGP
            var simpleLocation = episodeInfo.Title.Split(' ')[0].Trim();

            if (leadingSlash)
            {
                path = "/" + path;
            }
            updatedPath = path.Replace(searchTerm, $"Formula 1 {episodeInfo.SeasonNumber} {simpleLocation}");
            return true;
        }

        public bool ParseReleaseTitle(string releaseTitle, out string updatedTitle)
        {
            EnsureInitialized();
            updatedTitle = string.Empty;
            var match = F1AltReleaseRegex.Match(releaseTitle);
            if (!match.Success)
            {
                return false;
            }

            var year = int.Parse(match.Groups[1].Value);
            var round = int.Parse(match.Groups[2].Value);

            // Remainder contains location, session and metadata
            var remainder = match.Groups[3].Value;      

            // Gather location and session
            (string, int)[]  locations = Locations.Select(x => (x, Fuzz.PartialRatio(remainder, x))).OrderByDescending(x => x.Item2).Take(3).ToArray();
            (string, int)[] sessions = Sessions.Select(x => (x, Fuzz.PartialRatio(remainder, x))).OrderByDescending(x => x.Item2).Take(5).ToArray();

            // Find the best match for location and session
            var location = locations.First().Item1;
            var session = sessions.First().Item1;

            // If the session is Practice, Sprint or Qualifying and the second best match is > 85% then use the second best match
            // This is to avoid matching "Sprint" to "Sprint.Qualifying", "Practice" to "Practice 3", etc
            if ((session.Equals("Sprint", StringComparison.OrdinalIgnoreCase) ||
                session.Equals("Practice", StringComparison.OrdinalIgnoreCase) ||
                session.Equals("Qualifying", StringComparison.OrdinalIgnoreCase))
             && sessions[1].Item2 > 85)
            {
                session = sessions[1].Item1;
            }

            // find the matching episode in the season schedule
            var yearEpisodes = Episodes.Where(x => x.SeasonNumber == year);
            var fuzzySearch = $"{location} ({session})";

            var episodeMatch = yearEpisodes.OrderByDescending(x => Fuzz.Ratio(x.Title, fuzzySearch)).First();

            updatedTitle = $"Formula.1.S{year}E{episodeMatch.EpisodeNumber:00}.{episodeMatch.Title}.{releaseTitle}";
            return true;
        }

        /*
        public bool ParseReleaseTitle(string releaseTitle, out string? updatedTitle)
        {
            updatedTitle = null;
            var match = F1ReleaseRegex.Match(releaseTitle);
            if (!match.Success)
            {
                return false;
            }

            var year = int.Parse(match.Groups[1].Value);
            var round = int.Parse(match.Groups[2].Value);
            var location = match.Groups[3].Value;
            var type = match.Groups[4].Value;
            if (type == "Practice")
            {
                type = $"{type} {match.Groups[5].Value}";
            }

            // sprint shootout support
            if (type == "Sprint")
            {
                // if value 5 similar to shootout, then add it to the type
                if (Fuzz.Ratio(match.Groups[5].Value, "shootout") > 80)
                {
                    type = $"{type} {match.Groups[5].Value}";
                }            
            }

            // find the matching episode in the season schedule
            var episode = FindEpisode(round, year, location, type);
            if (episode == null)
            {
                return false;
            }
            updatedTitle = $"Formula.1.S{year}E{episode.EpisodeNumber:00}.{location}.{type}.{releaseTitle}";
            return true;
        }*/
    }
}