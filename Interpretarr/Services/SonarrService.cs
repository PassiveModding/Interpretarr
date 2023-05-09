using Interpretarr.Clients.QBittorrent;
using Interpretarr.Clients.Sonarr;
using Interpretarr.Model;

namespace Interpretarr.Services.F1
{
    public class SonarrService
    {

        public SonarrService(SonarrClient sonarrClient, QBittorrentClient qBittorrentClient, IEnumerable<IMiddlemanHelper> middlemanHelpers)
        {
            this.middlemanHelpers = middlemanHelpers;

            // create timer which calls on loop
            // delay start by 60 seconds
            // repeat every 60 seconds
            _timer = new Timer(CheckTorrents, null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));            
            this.sonarrClient = sonarrClient;
            this.qBittorrentClient = qBittorrentClient;
        }

        private readonly Timer _timer;
        private readonly SonarrClient sonarrClient;
        private readonly QBittorrentClient qBittorrentClient;
        private readonly IEnumerable<IMiddlemanHelper> middlemanHelpers;

        private async void CheckTorrents(object? state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            try
            {
                var queue = await sonarrClient.GetQueueAsync();
                var torrents = await qBittorrentClient.GetTorrentsAsync();

                foreach (var queueItem in queue)
                {
                    // Only process completed torrents
                    if (queueItem.Status != "completed" || queueItem.StatusMessages.Length == 0)
                    {
                        continue;
                    }

                    bool statusMessageMatch = false;

                    // Check if torrent failed import
                    foreach (var message in queueItem.StatusMessages)
                    {
                        if (message.Title == "One or more episodes expected in this release were not imported or missing")
                        {
                            statusMessageMatch = true;
                            break;
                        }
                        continue;
                    }

                    if (!statusMessageMatch) continue;

                    // rename torrent and all files to assist sonarr import
                    var torrent = torrents.FirstOrDefault(x => x.Name == queueItem.Title);
                    if (torrent == null) continue;

                    var files = await qBittorrentClient.GetTorrentFilesAsync(torrent.Hash);
                    foreach (var helper in middlemanHelpers)
                    {
                        // Check if the helper can process this torrent
                        if (!helper.ParseReleaseTitle(torrent.Name, out var newName))
                        {
                            continue;
                        }

                        await qBittorrentClient.RenameTorrentAsync(torrent.Hash, newName);

                        foreach (var file in files)
                        {
                            var pathSections = file.Name.Split("/");
                            for (int i = 0; i < pathSections.Length; i++)
                            {
                                var section = pathSections[i];
                                if (!helper.ParseReleaseTitle(section, out var updatedTitle))
                                {
                                    continue;
                                }

                                pathSections[i] = updatedTitle;
                            }

                            var path = string.Join("/", pathSections);
                            await qBittorrentClient.RenameFileAsync(torrent.Hash, file.Name, path);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
            }
            finally
            {
                _timer.Change(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
            }
        }
    }
}
