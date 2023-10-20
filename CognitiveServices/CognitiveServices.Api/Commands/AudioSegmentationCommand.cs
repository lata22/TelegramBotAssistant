using CognitiveServices.AI.Services.AudioSplitters;
using Infrastructure.Redis.CacheService;
using MediatR;

namespace CognitiveServices.Api.Commands;

public record AudioSegmentationCommand(string CacheKey) : IRequest<IEnumerable<string>>;

public class AudioSegmentationCommandHandler : IRequestHandler<AudioSegmentationCommand, IEnumerable<string>>
{
    private readonly ICacheWriterService<byte[]> _cacheWriterService;
    private readonly ICacheReaderService<byte[]> _cacheReaderService;
    private readonly IMP4AudioSplitter _audioSplitter;

    public AudioSegmentationCommandHandler(
        ICacheWriterService<byte[]> cacheWriterService,
        ICacheReaderService<byte[]> cacheReaderService,
        IMP4AudioSplitter audioSplitter)
    {
        _cacheWriterService = cacheWriterService;
        _cacheReaderService = cacheReaderService;
        _audioSplitter = audioSplitter;
    }

    public async Task<IEnumerable<string>> Handle(AudioSegmentationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var keys = request.CacheKey.Split(',');
            var audioStream = await CombineAudioFromCacheKeys(keys, cancellationToken) ??
            throw new ArgumentNullException($"Didn´t found in Redis the audio stream to split with CacheKey {request.CacheKey}");
            var audios = await _audioSplitter.SplitMP4Audio(audioStream, cancellationToken);
            var cacheKeys = new List<string>();
            for (int i = 0; i < audios.Count; i++)
            {
                string cacheKey = Guid.NewGuid().ToString();
                await _cacheWriterService.SetCacheValueAsync(cacheKey, audios[i], new TimeSpan(0, 20, 0));
                cacheKeys.Add(cacheKey);
            }
            return cacheKeys;

        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<byte[]> CombineAudioFromCacheKeys(IEnumerable<string> cacheKeys, CancellationToken cancellationToken)
    {
        List<byte[]> audioChunks = new List<byte[]>();

        // Read each byte array from Redis using its cache key
        foreach (var cacheKey in cacheKeys)
        {
            byte[]? audioChunk = await _cacheReaderService.GetCachedValueAsync(cacheKey);
            if (audioChunk != null)
            {
                audioChunks.Add(audioChunk);
            }
        }

        // Calculate the total length required for the final byte array
        int totalLength = audioChunks.Sum(arr => arr.Length);

        // Create a new byte array to hold all the chunks
        byte[] combinedAudio = new byte[totalLength];

        // Combine all the byte arrays into the final array
        int position = 0;
        foreach (var audioChunk in audioChunks)
        {
            Buffer.BlockCopy(audioChunk, 0, combinedAudio, position, audioChunk.Length);
            position += audioChunk.Length;
        }

        return combinedAudio;
    }
}



