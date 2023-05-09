namespace Interpretarr.Clients.Sonarr.Model.Series
{
    using System;
    using J = Newtonsoft.Json.JsonPropertyAttribute;
    using N = Newtonsoft.Json.NullValueHandling;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public partial class SeriesResource
    {
        [J("title")] public string Title { get; set; }
        [J("alternateTitles")] public AlternateTitle[] AlternateTitles { get; set; }
        [J("sortTitle")] public string SortTitle { get; set; }
        [J("status")] public string Status { get; set; }
        [J("ended")] public bool Ended { get; set; }
        [J("overview")] public string Overview { get; set; }
        [J("nextAiring")] public DateTimeOffset NextAiring { get; set; }
        [J("previousAiring")] public DateTimeOffset PreviousAiring { get; set; }
        [J("network")] public string Network { get; set; }
        [J("airTime")] public string AirTime { get; set; }
        [J("images")] public Image[] Images { get; set; }
        [J("seasons")] public Season[] Seasons { get; set; }
        [J("year")] public long Year { get; set; }
        [J("path")] public string Path { get; set; }
        [J("qualityProfileId")] public long QualityProfileId { get; set; }
        [J("languageProfileId")] public long LanguageProfileId { get; set; }
        [J("seasonFolder")] public bool SeasonFolder { get; set; }
        [J("monitored")] public bool Monitored { get; set; }
        [J("useSceneNumbering")] public bool UseSceneNumbering { get; set; }
        [J("runtime")] public long Runtime { get; set; }
        [J("tvdbId")] public long TvdbId { get; set; }
        [J("tvRageId")] public long TvRageId { get; set; }
        [J("tvMazeId")] public long TvMazeId { get; set; }
        [J("firstAired")] public DateTimeOffset FirstAired { get; set; }
        [J("seriesType")] public string SeriesType { get; set; }
        [J("cleanTitle")] public string CleanTitle { get; set; }
        [J("imdbId")] public string ImdbId { get; set; }
        [J("titleSlug")] public string TitleSlug { get; set; }
        [J("rootFolderPath")] public string RootFolderPath { get; set; }
        [J("genres")] public string[] Genres { get; set; }
        [J("tags")] public object[] Tags { get; set; }
        [J("added")] public DateTimeOffset Added { get; set; }
        [J("ratings")] public Ratings Ratings { get; set; }
        [J("statistics")] public Statistics Statistics { get; set; }
        [J("id")] public long Id { get; set; }
    }

    public partial class AlternateTitle
    {
        [J("title")] public string Title { get; set; }
        [J("seasonNumber")] public long SeasonNumber { get; set; }
    }

    public partial class Image
    {
        [J("coverType")] public string CoverType { get; set; }
        [J("url")] public string Url { get; set; }
        [J("remoteUrl")] public Uri RemoteUrl { get; set; }
    }

    public partial class Ratings
    {
        [J("votes")] public long Votes { get; set; }
        [J("value")] public long Value { get; set; }
    }

    public partial class Season
    {
        [J("seasonNumber")] public long SeasonNumber { get; set; }
        [J("monitored")] public bool Monitored { get; set; }
        [J("statistics")] public Statistics Statistics { get; set; }
    }

    public partial class Statistics
    {
        [J("previousAiring", NullValueHandling = N.Ignore)] public DateTimeOffset? PreviousAiring { get; set; }
        [J("episodeFileCount")] public long EpisodeFileCount { get; set; }
        [J("episodeCount")] public long EpisodeCount { get; set; }
        [J("totalEpisodeCount")] public long TotalEpisodeCount { get; set; }
        [J("sizeOnDisk")] public long SizeOnDisk { get; set; }
        [J("releaseGroups")] public object[] ReleaseGroups { get; set; }
        [J("percentOfEpisodes")] public long PercentOfEpisodes { get; set; }
        [J("nextAiring", NullValueHandling = N.Ignore)] public DateTimeOffset? NextAiring { get; set; }
        [J("seasonCount", NullValueHandling = N.Ignore)] public long? SeasonCount { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
