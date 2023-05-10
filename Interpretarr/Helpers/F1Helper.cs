using FuzzySharp;
using Interpretarr.Model;
using Interpretarr.Services;
using System.Text.RegularExpressions;

namespace Interpretarr.Helpers
{
    // Supports TVDB naming scheme 2014+ (last tested 2023-May)
    // Names:
    // Location (Practice 1)
    // Location (Practice 2)
    // Location (Practice 3)
    // Location (Qualifying)
    // Location (Sprint)
    // Location (Sprint Shootout)
    // Locaiton (Race)
    // NOTE: will not match testing episodes.
    public partial class F1Helper : IMiddlemanHelper
    {
        public F1Helper(F1DataProvider f1DataProvider, ILogger<F1Helper> logger)
        {
            this.f1DataProvider = f1DataProvider;
            this.logger = logger;
        }

        // Used to rewrite sonarr search from Formula 1 S2021E01 to Formula 1 2021 Bahrain
        public readonly Regex SonarrSearchKeywordRegex = new(@"^Formula\s+1\s+S(\d+)E(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // All matches capture groups: year, location, session
        public readonly Regex[] F1RegexList = new Regex[]
        {
            new(@"^Formula 1 (?<year>\d{4}) Round (?<round>\d+) (?<location>\w+) (?<remainder>.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"^Formula\s?1 (?<year>\d{4}) (?<location>.+) Grand Prix (?<remainder>.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"^Formula\s?1 (?<year>\d{4})x(?<round>\d+) (?<location>\w+) (?<remainder>.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"^Formula\s?1 (?<year>\d{4}) Round(?<round>\d+) (?<location>\w+) (?<remainder>.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"^Formula\.1\.(?<year>\d{4})\.Round\.(?<round>\d+)\.(?<location>\w+)\.(?<remainder>.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        private readonly F1DataProvider f1DataProvider;
        private readonly ILogger<F1Helper> logger;

        // Hardcoded list of session types to search for
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

            ("Practice.1", "Practice 1"),
            ("Practice.2", "Practice 2"),
            ("Practice.3", "Practice 3"),

            ("Qualifying", "Qualifying"),

            ("SQ", "Sprint Qualifying"),
            ("Sprint Qualifying", "Sprint Qualifying"),
            ("Sprint.Qualifying", "Sprint Qualifying"),
            ("SprintQualifying", "Sprint Qualifying"),

            ("Shootout", "Sprint Shootout"),
            ("Sprint Shootout", "Sprint Shootout"),
            ("SprintShootout", "Sprint Shootout"),
            ("Sprint.Shootout", "Sprint Shootout"),

            ("Sprint Race", "Sprint"),
            ("Sprint.Race", "Sprint"),
            ("Sprint", "Sprint"),

            ("Race", "Race")
        };

        public SupportedSite[] SupportedSites => new SupportedSite[] { SupportedSite._1337xTo, SupportedSite.TorrentLeech };

        public async Task<FormatResult> FormatKeywordsAsync(string keywords)
        {
            var match = SonarrSearchKeywordRegex.Match(keywords);
            if (!match.Success)
            {
                logger.LogDebug($"Failed to match {keywords} to {SonarrSearchKeywordRegex}");
                return new FormatResult(false, keywords);
            }            
            
            logger.LogTrace($"Matched {keywords} to {SonarrSearchKeywordRegex}");
            var season = int.Parse(match.Groups[1].Value);
            var episode = int.Parse(match.Groups[2].Value);

            var data = await f1DataProvider.GetAsync();
            var episodeInfo = data.Episodes.FirstOrDefault(x => x.SeasonNumber == season && x.EpisodeNumber == episode);
            if (episodeInfo == null)
            {
                return new FormatResult(false, keywords);
            }

            // take first word of title as location (ie abu for abu dhabi, united for united states, etc)
            // this narrows down the search results enough to find just the region.
            // the search results don't always have the full location or it is abbreviated differently. ie. Saudi Arabia vs. SaudiArabianGP
            var simpleLocation = episodeInfo.Title.Split(' ')[0].Trim();
            logger.LogDebug($"Matched {keywords} to {episodeInfo.Title} {simpleLocation}");
            return new FormatResult(true, $"Formula 1 {season} {simpleLocation}");
        }

        public async Task<FormatResult> FormatReleaseTitleAsync(string releaseTitle)
        {
            Match match = null!;
            foreach (var regex in F1RegexList)
            {
                match = regex.Match(releaseTitle);
                if (match.Success)
                {
                    logger.LogDebug($"Matched {releaseTitle} to {regex}");
                    break;
                }
            }

            if (match.Success == false)
            {
                logger.LogDebug($"No match found for {releaseTitle}");
                return new FormatResult(false, releaseTitle);
            }

            var year = int.Parse(match.Groups["year"].Value);
            var location = match.Groups["location"].Value;
            var remainder = match.Groups["remainder"].Value;      

            var data = await f1DataProvider.GetAsync();
            // Gather location and session
            (string, int) locationMatch = data.Locations.Select(x => (x, Fuzz.PartialRatio(location, x))).OrderByDescending(x => x.Item2).First();

            if (locationMatch.Item2 < 30)
            {
                logger.LogDebug($"Low location match score ({locationMatch.Item2}) for {location} - {releaseTitle}");
                return new FormatResult(false, releaseTitle);
            }

            logger.LogTrace($"Matched {location} to {locationMatch.Item1} ({locationMatch.Item2})");

            // session is a bit more complicated since it can be abbreviated in many ways
            // we need to find the best match for the session type
            // TODO: Also use Sessions list for this?
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
                logger.LogDebug($"No session match found for {releaseTitle}");
                return new FormatResult(false, releaseTitle);
            }

            // find the matching episode in the season schedule
            var yearEpisodes = data.Episodes.Where(x => x.SeasonNumber == year);
            var fuzzySearch = $"{location} ({sessionMatch})";

            var episodeMatch = yearEpisodes.OrderByDescending(x => Fuzz.Ratio(x.Title, fuzzySearch)).First();

            logger.LogDebug($"Matched {releaseTitle} to {episodeMatch.Title} {sessionMatch}");
            return new FormatResult(true, $"Formula 1 S{year}E{episodeMatch.EpisodeNumber:00} {episodeMatch.Title} {releaseTitle}");
        }
    }
}