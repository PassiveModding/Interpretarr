namespace Interpretarr.Clients.QBittorrent.Model
{
    using J = Newtonsoft.Json.JsonPropertyAttribute;
    using N = Newtonsoft.Json.NullValueHandling;

    public partial class TorrentFileResource
    {
        [J("availability")] public long Availability { get; set; }
        [J("index")] public long Index { get; set; }
        [J("is_seed", NullValueHandling = N.Ignore)] public bool? IsSeed { get; set; }
        [J("name")] public string Name { get; set; }
        [J("piece_range")] public long[] PieceRange { get; set; }
        [J("priority")] public long Priority { get; set; }
        [J("progress")] public long Progress { get; set; }
        [J("size")] public long Size { get; set; }
    }
}
