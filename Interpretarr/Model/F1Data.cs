using Interpretarr.Clients.Sonarr.Model.Episode;

namespace Interpretarr.Model
{
    public class F1Data
    {
        public F1Data(EpisodeResource[] episodes, HashSet<string> locations, HashSet<string> sessions)
        {
            Episodes = episodes;
            Locations = locations;
            Sessions = sessions;
        }

        public EpisodeResource[] Episodes { get; set; }

        public HashSet<string> Locations { get; set; }

        // Sessions gathered from parsing Sonarr episode titles
        public HashSet<string> Sessions { get; set; }
    }
}