using CognitiveServices.AI.Services.Youtube;
using Infrastructure.Redis.CacheService;
using MediatR;
using YoutubeExplode.Videos;

namespace CognitiveServices.Api.Commands.YoutubeVideo;
public record YoutubeDownloadResult(AI.Services.Youtube.YoutubeVideo YoutubeVideo, string CacheKey);

public record YoutubeDownloadCommand(Guid CorrelationId, string VideoUrl) : IRequest<YoutubeDownloadResult>;
public class YoutubeDownloadCommandHandler : IRequestHandler<YoutubeDownloadCommand, YoutubeDownloadResult>
{
    private readonly IYoutubeVideoDownloader _youtubeVideoDownloader;
    private readonly IYoutubeClosedCaptionDownloader _youtubeClosedCaptionDownloader;
    private readonly ICacheWriterService<byte[]> _cacheWriterService;
    public YoutubeDownloadCommandHandler(
        IYoutubeVideoDownloader youtubeVideoDownloader,
        IYoutubeClosedCaptionDownloader youtubeClosedCaptionDownloader,
        ICacheWriterService<byte[]> cacheWriterService)
    {
        _youtubeVideoDownloader = youtubeVideoDownloader;
        _youtubeClosedCaptionDownloader = youtubeClosedCaptionDownloader;
        _cacheWriterService = cacheWriterService;
    }

    public async Task<YoutubeDownloadResult> Handle(YoutubeDownloadCommand request, CancellationToken cancellationToken)
    {
        string id = request.VideoUrl.Split("/").Last().Split("?").First();
        var videoId = VideoId.TryParse(request.VideoUrl) ??
            VideoId.TryParse($"https://www.youtube.com/watch?v={id}");
        if (videoId == null)
        {
            throw new ArgumentNullException("Could not parse VideoId");
        }
        AI.Services.Youtube.YoutubeVideo youtubeVideo = default!;
        string cacheKeys = string.Empty;
        var youtubeClient = new YoutubeExplode.YoutubeClient();
        youtubeVideo = await _youtubeClosedCaptionDownloader
            .DownloadCaptionAsync(youtubeClient, (VideoId)videoId, cancellationToken);

        if (string.IsNullOrWhiteSpace(youtubeVideo.ClosedCaptions))
        {
            youtubeVideo = await _youtubeVideoDownloader
                .DownloadVideo(youtubeClient, request.VideoUrl, cancellationToken);
            if (youtubeVideo.AudioStreams.Count > 0)
            {

                foreach (var stream in youtubeVideo.AudioStreams)
                {
                    string cacheKey = Guid.NewGuid().ToString();
                    await _cacheWriterService.SetCacheValueAsync(cacheKey, stream.ToArray());
                    cacheKeys += cacheKey + ",";
                }
            }

        }
        return new YoutubeDownloadResult(youtubeVideo ?? new AI.Services.Youtube.YoutubeVideo(new List<MemoryStream>(), string.Empty, string.Empty, string.Empty, string.Empty), cacheKeys);
    }
}

