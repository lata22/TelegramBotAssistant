using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Videos;

namespace CognitiveServices.AI.Services.Youtube;

public interface IYoutubeVideoDownloader
{
    Task<YoutubeVideo> DownloadVideo(YoutubeClient youtubeClient, string url, CancellationToken cancellationToken);
}

public class YoutubeVideoDownloader : IYoutubeVideoDownloader
{
    private readonly IAudioStreamDownloader _audioStreamdownloader;

    public YoutubeVideoDownloader(IAudioStreamDownloader audioStreamdownloader)
    {
        _audioStreamdownloader = audioStreamdownloader;
    }

    public async Task<YoutubeVideo> DownloadVideo(YoutubeClient youtubeClient, string url, CancellationToken cancellationToken)
    {
        var videoId = VideoId.TryParse(url);
        if (videoId == null)
        {
            videoId = ExtractVideoId(url);
        }
        var memoryStreams = new List<MemoryStream>();
        string videoTitle = string.Empty;
        string videoAuthor = string.Empty;

        if (videoId != null)
        {
            var video = await youtubeClient.Videos.GetAsync((VideoId)videoId, cancellationToken);
            videoTitle = video.Title;
            videoAuthor = video.Author.ChannelTitle;
            var streamManifest = await youtubeClient.Videos.Streams
                .GetManifestAsync(videoId.Value, cancellationToken);
            var streamInfo = streamManifest
                .GetAudioOnlyStreams()
                .OrderByDescending(ao => ao.Size)
                .First();

            if (streamInfo is not null)
            {
                var invalidChars = new[] { '<', '>', ':', '"', '|', '?', '*' };
                videoTitle = new string(videoTitle.Where(ch => !invalidChars.Contains(ch)).ToArray());
                var youtubeStream = await youtubeClient.Videos.Streams.GetAsync(streamInfo, cancellationToken);
                memoryStreams = await _audioStreamdownloader.GetVideoBytesAsync(youtubeStream, cancellationToken);
            }
        }
        var youtubeVideo = new YoutubeVideo(memoryStreams, videoTitle, url, videoAuthor, string.Empty);
        return youtubeVideo;
    }

    private string? ExtractVideoId(string url)
    {
        // Regex pattern to match video ID in various YouTube URL formats
        var regexPattern = @"youtube\.com/live/([\w-]+)";
        var match = Regex.Match(url, regexPattern);

        if (match.Success)
            return match.Groups[1].Value;

        return null;
    }
}