namespace CognitiveServices.AI.Services.Youtube;

public interface IAudioStreamDownloader
{
    Task<List<MemoryStream>> GetVideoBytesAsync(Stream largeStream, CancellationToken cancellationToken, int chunkSize = 104857600);
}
