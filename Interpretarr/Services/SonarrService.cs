using Interpretarr.Clients.QBittorrent;
using Interpretarr.Clients.QBittorrent.Model;
using Interpretarr.Clients.Sonarr;
using Interpretarr.Model;

namespace Interpretarr.Services
{
    public class SonarrService
    {

        public SonarrService(SonarrClient sonarrClient, QBittorrentClient qBittorrentClient, IEnumerable<IMiddlemanHelper> middlemanHelpers)
        {
            this.middlemanHelpers = middlemanHelpers;

            // create timer which calls on loop
            // delay start by 60 seconds
            // repeat every 60 seconds
            _timer = new Timer(CheckTorrents, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(60));
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
                await ProcessTorrents();
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

        private async Task ProcessTorrents()
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

                // Check if torrent failed import
                if (!SonarrStatusIsFailedImport(queueItem.StatusMessages))
                {
                    continue;
                }

                // rename torrent and all files to assist sonarr import
                var torrent = torrents.FirstOrDefault(x => x.Name == queueItem.Title);
                if (torrent == null) continue;

                await ProcessTorrentFilesAsync(torrent);
            }
        }

        private bool SonarrStatusIsFailedImport(Clients.Sonarr.Model.Queue.StatusMessage[] statusMessages)
        {
            foreach (var message in statusMessages)
            {
                if (message.Title == "One or more episodes expected in this release were not imported or missing")
                {
                    return true;
                }
            }

            return false;
        }

        private async Task ProcessTorrentFilesAsync(TorrentInfoResource torrent)
        {
            var files = await qBittorrentClient.GetTorrentFilesAsync(torrent.Hash);
            foreach (var helper in middlemanHelpers)
            {
                var titleResult = await helper.FormatReleaseTitleAsync(torrent.Name);
                if (!titleResult.Success || titleResult.Result == null)
                {
                    continue;
                }

                await qBittorrentClient.RenameTorrentAsync(torrent.Hash, titleResult.Result);

                foreach (var file in files)
                {
                    var pathSections = file.Name.Split("/");
                    for (int i = 0; i < pathSections.Length; i++)
                    {
                        var section = pathSections[i];
                        var sectionResult = await helper.FormatReleaseTitleAsync(section);
                        if (!sectionResult.Success || sectionResult.Result == null)
                        {
                            continue;
                        }

                        pathSections[i] = sectionResult.Result;
                    }

                    var path = string.Join("/", pathSections);
                    await qBittorrentClient.RenameFileAsync(torrent.Hash, file.Name, path);
                }
            }
        }

    }
}
