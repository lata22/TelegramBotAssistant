namespace CognitiveServices.AI.Services.Youtube;

public class AudioStreamDownloader : IAudioStreamDownloader
{
    public async Task<List<MemoryStream>> GetVideoBytesAsync(Stream largeStream, CancellationToken cancellationToken, int chunkSize = 104857600)
    {
        var memoryStreams = new List<MemoryStream>();
        byte[] buffer = new byte[chunkSize];
        int bytesRead;

        while ((bytesRead = await largeStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        {
            // Initialize a new MemoryStream for each chunk
            MemoryStream memoryStream = new MemoryStream();

            // Write the read bytes into the MemoryStream
            await memoryStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);

            // Reset the position to the beginning of the MemoryStream
            memoryStream.Position = 0;

            // Add it to the list
            memoryStreams.Add(memoryStream);
        }

        return memoryStreams;
    }
}
