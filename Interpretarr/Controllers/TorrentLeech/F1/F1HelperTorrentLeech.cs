using FuzzySharp;
using HtmlAgilityPack;
using Interpretarr.Clients.Sonarr;
using Interpretarr.Clients.Sonarr.Model.Episode;
using Interpretarr.Clients.TorrentLeech.Model;
using Interpretarr.Model;
using Newtonsoft.Json;
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
    public partial class F1HelperTorrentLeech : IMiddlemanHelper
    {
        private const int tvdbId = 387219;

        private EpisodeResource[] Episodes { get; set; }
        private HashSet<string> Locations { get; set; } = new();
        private HashSet<string> Sessions { get; set; } = new();

        public SupportedSite SupportedSite => SupportedSite.TorrentLeech;

        public SonarrClient SonarrClient { get; }

        // Used to parse sonarr title from Bahrain (Practice 1)
        public readonly Regex TitleRegex = new(@"^(.+) \((.+)\)$", RegexOptions.Compiled);

        // Used to rewrite sonarr search from Formula 1 S2021E01 to Formula 1 2021 Bahrain
        public readonly Regex SonarrSearchRegex = new(@"^Formula\s+1\s+S(\d+)E(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        public readonly Regex[] F1RegexList = new Regex[]
        {
            new(@"^Formula 1 (\d{4}) Round \d+ (\w+) (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"^Formula\s?1 (\d{4}) (.+) Grand Prix (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"^Formula\s?1 (\d{4})x\d+ (\w+) (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"^Formula\s?1 (\d{4}) Round\d+ (\w+) (.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };

        public List<(string, string)> SessionTypes = new List<(string, string)>
        {
            ("FP1", "Practice 1"),
            ("FP2", "Practice 2"),
            ("FP3", "Practice 3"),

            ("Practice One", "Practice 1"),
            ("Practice Two", "Practice 2"),
            ("Practice Three", "Practice 3"),

            ("Practice 1", "Practice 1"),
            ("Practice 2", "Practice 2"),
            ("Practice 3", "Practice 3"),

            ("Qualifying", "Qualifying"),

            ("SQ", "Sprint Qualifying"),
            ("Sprint Qualifying", "Sprint Qualifying"),
            ("SprintQualifying", "Sprint Qualifying"),

            ("Shootout", "Sprint Shootout"),
            ("Sprint Shootout", "Sprint Shootout"),
            ("SprintShootout", "Sprint Shootout"),

            ("Sprint Race", "Sprint"),
            ("Sprint", "Sprint"),
            ("Race", "Race")
        };


        public F1HelperTorrentLeech(SonarrClient sonarrClient)
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
            var torrentLeechResponse = JsonConvert.DeserializeObject<Query>(body);
            if (torrentLeechResponse == null)
            {
                return body;
            }

            foreach (var torrent in torrentLeechResponse.TorrentList)
            {
                if (ParseReleaseTitle(torrent.Name, out var updatedTitle))
                {
                    torrent.Name = updatedTitle;
                }
            }

            // serialize but keep any fields that were excluded in serializing to query since body typically contains extra fields that are required
            var bodyObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(body);
            bodyObject["torrentList"] = torrentLeechResponse.TorrentList;
            return JsonConvert.SerializeObject(bodyObject);
        }

        // .../torrents/browse/index/categories/26,32/facets/tags%3ASports/query/formula%201 S2023E01/orderby/added/order/desc
        public bool HandleRequest(string path, out string updatedPath)
        {
            EnsureInitialized();
            updatedPath = path;
            // remove the leading slash if it exists, return it at the end.
            var initialPath = path;
            var leadingSlash = path.StartsWith('/');
            path = path.TrimStart('/');
            if (!path.StartsWith("torrents/browse/list/categories/", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // parse out the search term by getting the value after /query/ and before /orderby/
            var searchMatch = Regex.Match(path, @"query/(.+?)/");
            if (!searchMatch.Success)
            {
                return false;
            }
            
            var searchTerm = searchMatch.Groups[1].Value;
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

        // TODO: 
        // remove YYYYxRR from title to avoid false matches if sonarr detects round as episode number
        // releaseTitle = Regex.Replace(releaseTitle, @"(\d{4})x(\d+)", "$1 $2");

        public bool ParseReleaseTitle(string releaseTitle, out string updatedTitle)
        {
            EnsureInitialized();
            updatedTitle = string.Empty;
            
            Match match = null!;
            foreach (var regex in F1RegexList)
            {
                match = regex.Match(releaseTitle);
                if (match.Success) break;
            }

            if (match.Success == false)
            {
                return false;
            }

            var year = int.Parse(match.Groups[1].Value);
            //var round = int.Parse(match.Groups[2].Value);
            var location = match.Groups[2].Value;
            // Remainder contains session and metadata
            var remainder = match.Groups[3].Value;      

            // Gather location and session
            (string, int) locationMatch = Locations.Select(x => (x, Fuzz.PartialRatio(location, x))).OrderByDescending(x => x.Item2).First();

            // session is a bit more complicated since it can be abbreviated in many ways
            // we need to find the best match for the session type
            var sessionMatches = new List<String>();
            foreach (var sessionType in SessionTypes)
            {
                if (remainder.Contains(sessionType.Item1))
                {
                    sessionMatches.Add(sessionType.Item2);
                }
            }

            var sessionMatch = sessionMatches.OrderByDescending(x => x.Length).FirstOrDefault();
            if (sessionMatch == null)
            {
                return false;
            }

            // find the matching episode in the season schedule
            var yearEpisodes = Episodes.Where(x => x.SeasonNumber == year);
            var fuzzySearch = $"{location} ({sessionMatch})";

            var episodeMatch = yearEpisodes.OrderByDescending(x => Fuzz.Ratio(x.Title, fuzzySearch)).First();

            updatedTitle = $"Formula 1 S{year}E{episodeMatch.EpisodeNumber:00} {episodeMatch.Title} {releaseTitle}";
            return true;
        }
    }
}