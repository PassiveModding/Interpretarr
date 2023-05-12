using FuzzySharp;
using Interpretarr.Clients.Sonarr.Model.Episode;
using Interpretarr.Model;
using Interpretarr.Services;
using System.Text.RegularExpressions;

namespace Interpretarr.Helpers
{
    // Tested against 2022/2023 seasons
    public partial class IndyCarHelper : IMiddlemanHelper
    {
        public IndyCarHelper(IndyCarDataProvider indyCarDataProvider, ILogger<IndyCarHelper> logger)
        {
            this.indyCarDataProvider = indyCarDataProvider;
            this.logger = logger;
        }

        public readonly Regex SonarrSearchKeywordRegex = new(@"^NTT\s+Indycar\s+Series\s+S(\d+)E(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public readonly Regex[] RegexList = new Regex[]
        {
            new(@"^NTT Indycar Series (?<year>\d{4}) (?<title>.+) HDTV.*$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"^NTT Indycar (?<year>\d{4}) (?<title>.+) HDTV.*$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        };
        private readonly IndyCarDataProvider indyCarDataProvider;
        private readonly ILogger<IndyCarHelper> logger;

        public SupportedSite[] SupportedSites => new SupportedSite[] { SupportedSite.TorrentLeech };

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

            var episodes = await indyCarDataProvider.GetAsync();
            // TODO: Verify episode number matches round nuber after 12/13 where the round number may be different from the episode number
            var episodeInfo = episodes.FirstOrDefault(x => x.SeasonNumber == season && x.EpisodeNumber == episode);
            if (episodeInfo == null)
            {
                return new FormatResult(false, keywords);
            }

            logger.LogDebug($"Matched {keywords} to {episodeInfo.Title}");
            var simpleTerm = episodeInfo.Title.Split(" ").First();
            var searchTerm = $"NTT INDYCAR {season} {simpleTerm}";
            logger.LogDebug($"Updated search term {keywords} to {searchTerm}");
            return new FormatResult(true, searchTerm);
        }

        public async Task<FormatResult> FormatReleaseTitleAsync(string releaseTitle)
        {
            Match match = null!;
            foreach (var regex in RegexList)
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
            var title = match.Groups["title"].Value;    

            var episodes = await indyCarDataProvider.GetAsync();
            // Gather location and session
            (EpisodeResource, int) episodeMatch = episodes.Where(x => x.SeasonNumber == year).Select(x => (x, Fuzz.PartialRatio(title, x.Title))).OrderByDescending(x => x.Item2).First();

            if (episodeMatch.Item2 < 30)
            {
                logger.LogDebug($"Low location match score ({episodeMatch.Item2}) for {title} - {releaseTitle}");
                return new FormatResult(false, releaseTitle);
            }

            logger.LogTrace($"Matched {title} to {episodeMatch.Item1.Title} ({episodeMatch.Item2})");
            return new FormatResult(true, $"NTT IndyCar S{year}E{episodeMatch.Item1.EpisodeNumber:00} {title}");
        }
    }
}