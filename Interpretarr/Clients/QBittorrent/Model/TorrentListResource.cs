namespace Interpretarr.Clients.QBittorrent.Model
{
    using J = Newtonsoft.Json.JsonPropertyAttribute;

    public partial class TorrentInfoResource
    {
        [J("added_on")] public long AddedOn { get; set; }
        [J("amount_left")] public long AmountLeft { get; set; }
        [J("auto_tmm")] public bool AutoTmm { get; set; }
        [J("availability")] public long Availability { get; set; }
        [J("category")] public string Category { get; set; }
        [J("completed")] public long Completed { get; set; }
        [J("completion_on")] public long CompletionOn { get; set; }
        [J("content_path")] public string ContentPath { get; set; }
        [J("dl_limit")] public long DlLimit { get; set; }
        [J("dlspeed")] public long Dlspeed { get; set; }
        [J("download_path")] public string DownloadPath { get; set; }
        [J("downloaded")] public long Downloaded { get; set; }
        [J("downloaded_session")] public long DownloadedSession { get; set; }
        [J("eta")] public long Eta { get; set; }
        [J("f_l_piece_prio")] public bool FLPiecePrio { get; set; }
        [J("force_start")] public bool ForceStart { get; set; }
        [J("hash")] public string Hash { get; set; }
        [J("infohash_v1")] public string InfohashV1 { get; set; }
        [J("infohash_v2")] public string InfohashV2 { get; set; }
        [J("last_activity")] public long LastActivity { get; set; }
        [J("magnet_uri")] public string MagnetUri { get; set; }
        [J("max_ratio")] public long MaxRatio { get; set; }
        [J("max_seeding_time")] public long MaxSeedingTime { get; set; }
        [J("name")] public string Name { get; set; }
        [J("num_complete")] public long NumComplete { get; set; }
        [J("num_incomplete")] public long NumIncomplete { get; set; }
        [J("num_leechs")] public long NumLeechs { get; set; }
        [J("num_seeds")] public long NumSeeds { get; set; }
        [J("priority")] public long Priority { get; set; }
        [J("progress")] public long Progress { get; set; }
        [J("ratio")] public long Ratio { get; set; }
        [J("ratio_limit")] public long RatioLimit { get; set; }
        [J("save_path")] public string SavePath { get; set; }
        [J("seeding_time")] public long SeedingTime { get; set; }
        [J("seeding_time_limit")] public long SeedingTimeLimit { get; set; }
        [J("seen_complete")] public long SeenComplete { get; set; }
        [J("seq_dl")] public bool SeqDl { get; set; }
        [J("size")] public long Size { get; set; }
        [J("state")] public string State { get; set; }
        [J("super_seeding")] public bool SuperSeeding { get; set; }
        [J("tags")] public string Tags { get; set; }
        [J("time_active")] public long TimeActive { get; set; }
        [J("total_size")] public long TotalSize { get; set; }
        [J("tracker")] public string Tracker { get; set; }
        [J("trackers_count")] public long TrackersCount { get; set; }
        [J("up_limit")] public long UpLimit { get; set; }
        [J("uploaded")] public long Uploaded { get; set; }
        [J("uploaded_session")] public long UploadedSession { get; set; }
        [J("upspeed")] public long Upspeed { get; set; }
    }
}
