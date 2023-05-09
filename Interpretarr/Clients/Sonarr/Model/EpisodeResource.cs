namespace Interpretarr.Clients.Sonarr.Model.Episode
{
    using System;
    using J = Newtonsoft.Json.JsonPropertyAttribute;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public partial class EpisodeResource
    {
        [J("seriesId")] public long SeriesId { get; set; }
        [J("tvdbId")] public long TvdbId { get; set; }
        [J("episodeFileId")] public long EpisodeFileId { get; set; }
        [J("seasonNumber")] public long SeasonNumber { get; set; }
        [J("episodeNumber")] public long EpisodeNumber { get; set; }
        [J("title")] public string Title { get; set; }
        [J("airDate")] public DateTimeOffset AirDate { get; set; }
        [J("airDateUtc")] public DateTimeOffset AirDateUtc { get; set; }
        [J("hasFile")] public bool HasFile { get; set; }
        [J("monitored")] public bool Monitored { get; set; }
        [J("unverifiedSceneNumbering")] public bool UnverifiedSceneNumbering { get; set; }
        [J("id")] public long Id { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
