namespace Interpretarr.Clients.TorrentLeech.Model
{
    using System;
    using J = Newtonsoft.Json.JsonPropertyAttribute;

    public partial class Query
    {
        [J("numFound")]       public long NumFound { get; set; }            
        [J("torrentList")]    public TorrentList[] TorrentList { get; set; }
        [J("order")]          public string Order { get; set; }             
        [J("orderBy")]        public string OrderBy { get; set; }           
        [J("page")]           public long Page { get; set; }                
        [J("perPage")]        public long PerPage { get; set; }             
        [J("lastBrowseTime")] public string LastBrowseTime { get; set; }    
        [J("userTimeZone")]   public string UserTimeZone { get; set; }      
    }

    public partial class TorrentList
    {
        [J("fid")]                 public string Fid { get; set; }                   
        [J("filename")]            public string Filename { get; set; }              
        [J("name")]                public string Name { get; set; }                  
        [J("addedTimestamp")]      public DateTimeOffset AddedTimestamp { get; set; }
        [J("categoryID")]          public long CategoryId { get; set; }              
        [J("size")]                public long Size { get; set; }                    
        [J("completed")]           public long Completed { get; set; }               
        [J("seeders")]             public long Seeders { get; set; }                 
        [J("leechers")]            public long Leechers { get; set; }                
        [J("numComments")]         public long NumComments { get; set; }             
        [J("tags")]                public string[] Tags { get; set; }                
        [J("new")]                 public bool New { get; set; }                     
        [J("imdbID")]              public string ImdbId { get; set; }                
        [J("rating")]              public long Rating { get; set; }                  
        [J("genres")]              public string Genres { get; set; }                
        [J("tvmazeID")]            public string TvmazeId { get; set; }              
        [J("igdbID")]              public string IgdbId { get; set; }                
        [J("download_multiplier")] public long DownloadMultiplier { get; set; }      
    }
}
