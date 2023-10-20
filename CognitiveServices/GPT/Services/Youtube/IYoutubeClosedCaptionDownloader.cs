using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;

namespace CognitiveServices.AI.Services.Youtube;


public interface IYoutubeClosedCaptionDownloader
{
    Task<YoutubeVideo> DownloadCaptionAsync(YoutubeClient youtubeClient, VideoId videoId, CancellationToken cancellationToken);
}


public class YoutubeClosedCaptionDownloader : IYoutubeClosedCaptionDownloader
{
    public async Task<YoutubeVideo> DownloadCaptionAsync(YoutubeClient youtubeClient, VideoId videoId, CancellationToken cancellationToken)
    {
        var video = await youtubeClient.Videos.GetAsync(videoId, cancellationToken);
        var videoTitle = video.Title;
        var videoAuthor = video.Author.ChannelTitle;
        string closedCaptions = string.Empty;

        var closedCaptionsManifest = await youtubeClient.Videos.ClosedCaptions
            .GetManifestAsync(videoId, cancellationToken);
        var closedCaptionTrackInfo = GetClosedCaption(closedCaptionsManifest);
        if (closedCaptionTrackInfo is not null)
        {
            var closedCaptionTrack = await youtubeClient.Videos.ClosedCaptions.GetAsync(closedCaptionTrackInfo, cancellationToken);
            closedCaptions = string.Join(" ", closedCaptionTrack.Captions.Select(cc => cc.Text));
        }

        return new YoutubeVideo(new List<MemoryStream>(), videoTitle, video.Url, videoAuthor, closedCaptions);
    }

    private ClosedCaptionTrackInfo? GetClosedCaption(ClosedCaptionManifest manifest)
    {
        ClosedCaptionTrackInfo? result = null;
        string[] language = { "en", "es" };
        language.ToList().ForEach(language =>
        {
            try
            {
                result = manifest.GetByLanguage(language);
            }
            catch { }
        });
        return result;
    }
}