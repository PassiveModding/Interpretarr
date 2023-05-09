namespace Interpretarr.Clients.Sonarr.Model.Queue
{
    using J = Newtonsoft.Json.JsonPropertyAttribute;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public partial class QueueResource
    {
        [J("seriesId")] public long SeriesId { get; set; }
        [J("episodeId")] public long EpisodeId { get; set; }
        [J("episode")] public Episode Episode { get; set; }
        [J("language")] public Language Language { get; set; }
        [J("quality")] public QueueResourceQuality Quality { get; set; }
        [J("size")] public long Size { get; set; }
        [J("title")] public string Title { get; set; }
        [J("sizeleft")] public long Sizeleft { get; set; }
        //[J("timeleft")] public DateTimeOffset Timeleft { get; set; }
        //[J("estimatedCompletionTime")] public DateTimeOffset EstimatedCompletionTime { get; set; }
        [J("status")] public string Status { get; set; }
        [J("trackedDownloadStatus")] public string TrackedDownloadStatus { get; set; }
        [J("trackedDownloadState")] public string TrackedDownloadState { get; set; }
        [J("statusMessages")] public StatusMessage[] StatusMessages { get; set; }
        [J("downloadId")] public string DownloadId { get; set; }
        [J("protocol")] public string Protocol { get; set; }
        [J("downloadClient")] public string DownloadClient { get; set; }
        [J("indexer")] public string Indexer { get; set; }
        [J("outputPath")] public string OutputPath { get; set; }
        [J("id")] public long Id { get; set; }
    }

    public partial class Episode
    {
        [J("seriesId")] public long SeriesId { get; set; }
        [J("tvdbId")] public long TvdbId { get; set; }
        [J("episodeFileId")] public long EpisodeFileId { get; set; }
        [J("seasonNumber")] public long SeasonNumber { get; set; }
        [J("episodeNumber")] public long EpisodeNumber { get; set; }
        [J("title")] public string Title { get; set; }
        //[J("airDate")] public DateTimeOffset AirDate { get; set; }
        //[J("airDateUtc")] public DateTimeOffset AirDateUtc { get; set; }
        [J("hasFile")] public bool HasFile { get; set; }
        [J("monitored")] public bool Monitored { get; set; }
        [J("unverifiedSceneNumbering")] public bool UnverifiedSceneNumbering { get; set; }
        [J("id")] public long Id { get; set; }
    }

    public partial class Language
    {
        [J("id")] public long Id { get; set; }
        [J("name")] public string Name { get; set; }
    }

    public partial class QueueResourceQuality
    {
        [J("quality")] public QualityQuality Quality { get; set; }
        [J("revision")] public Revision Revision { get; set; }
    }

    public partial class QualityQuality
    {
        [J("id")] public long Id { get; set; }
        [J("name")] public string Name { get; set; }
        [J("source")] public string Source { get; set; }
        [J("resolution")] public long Resolution { get; set; }
    }

    public partial class Revision
    {
        [J("version")] public long Version { get; set; }
        [J("real")] public long Real { get; set; }
        [J("isRepack")] public bool IsRepack { get; set; }
    }

    public partial class StatusMessage
    {
        [J("title")] public string Title { get; set; }
        [J("messages")] public string[] Messages { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
