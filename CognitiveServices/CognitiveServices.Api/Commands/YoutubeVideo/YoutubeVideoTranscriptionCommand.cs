using CognitiveServices.AI.Services.Whisper;
using Infrastructure.Redis.CacheService;
using MediatR;
using System.Collections.Concurrent;

namespace CognitiveServices.Api.Commands.YoutubeVideo
{
    public record YoutubeVideoTranscriptionCommand(IEnumerable<string> CacheKeys) : IRequest<string>;
    public class YoutubeVideoTranscriptionCommandHandler : IRequestHandler<YoutubeVideoTranscriptionCommand, string>
    {
        private readonly ICacheReaderService<byte[]> _cacheReaderService;
        private readonly IWhisperService _whisperService;

        public YoutubeVideoTranscriptionCommandHandler(
            ICacheReaderService<byte[]> cacheReaderService,
            IWhisperService whisperService)
        {
            _cacheReaderService = cacheReaderService;
            _whisperService = whisperService;
        }

        public async Task<string> Handle(YoutubeVideoTranscriptionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var semaphore = new SemaphoreSlim(5);
                var audios = new ConcurrentBag<(byte[] Audio, int Index)>();
                await Task.WhenAll(request.CacheKeys.Select(async (key, index) =>
                {
                    await semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        var audioBytes = await _cacheReaderService.GetCachedValueAsync(key);
                        if (audioBytes != null)
                        {
                            audios.Add((audioBytes, index));
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
                var sortedAudios = audios.OrderBy(tuple => tuple.Index).Select(tuple => tuple.Audio).ToList();
                var whisperResponse = await _whisperService.CreateTranscription(sortedAudios, string.Empty, cancellationToken);
                string whisperTranscription = string.Join(string.Empty, whisperResponse.Select(wr => wr.Text).ToList());
                return whisperTranscription;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

}
