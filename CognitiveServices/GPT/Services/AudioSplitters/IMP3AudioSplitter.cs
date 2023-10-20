using NAudio.Lame;
using NAudio.Wave;

namespace CognitiveServices.AI.Services.AudioSplitters
{
    public interface IMP3AudioSplitter
    {
        Task<List<byte[]>> SplitMP3Audio(byte[] mp3Audio, double segmentDurationInSeconds, int targetBitRate, CancellationToken cancellationToken);
    }

    public class MP3AudioSplitter : IMP3AudioSplitter
    {
        public async Task<List<byte[]>> SplitMP3Audio(byte[] mp3Audio, double segmentDurationInSeconds, int targetBitRate, CancellationToken cancellationToken)
        {
            int totalSegments = CalculateTotalSegments(mp3Audio, segmentDurationInSeconds);
            var tasks = new Task<byte[]>[totalSegments];
            var audioSegments = new List<byte[]>(totalSegments);

            for (int i = 0; i < totalSegments; i++)
            {
                // Create a task to split each segment asynchronously
                tasks[i] = CreateSegment(mp3Audio, i, segmentDurationInSeconds, targetBitRate, cancellationToken);
            }

            // Await all the tasks to complete
            await Task.WhenAll(tasks);

            // Add the completed segments in the correct order
            for (int i = 0; i < totalSegments; i++)
            {
                audioSegments.Add(tasks[i].Result);
            }

            return audioSegments;
        }

        public async Task<byte[]> CreateSegment(byte[] mp3Audio, int segmentNumber, double segmentDurationInSeconds, int targetBitRate, CancellationToken cancellationToken)
        {
            using (var reader = new Mp3FileReader(new MemoryStream(mp3Audio)))
            {
                long startPosition = (long)(segmentDurationInSeconds * segmentNumber * reader.WaveFormat.AverageBytesPerSecond);
                reader.Position = startPosition;

                var buffer = new byte[reader.WaveFormat.AverageBytesPerSecond];
                using (var memoryStream = new MemoryStream())
                {
                    using (var writer = new LameMP3FileWriter(memoryStream, reader.WaveFormat, targetBitRate))
                    {
                        double segmentDurationInMilliseconds = segmentDurationInSeconds * 1000;
                        double writtenMilliseconds = 0;

                        while (writtenMilliseconds < segmentDurationInMilliseconds && reader.Position < reader.Length)
                        {
                            int bytesToRead = (int)Math.Min(buffer.Length, (segmentDurationInMilliseconds - writtenMilliseconds) / 1000.0 * reader.WaveFormat.AverageBytesPerSecond);
                            int bytesRead = await reader.ReadAsync(buffer, 0, bytesToRead, cancellationToken);
                            await writer.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                            writtenMilliseconds += bytesRead / (double)reader.WaveFormat.AverageBytesPerSecond * 1000.0;
                        }
                    }

                    return memoryStream.ToArray();
                }
            }
        }

        public int CalculateTotalSegments(byte[] audio, double segmentDurationInSeconds)
        {
            using (var reader = new Mp3FileReader(new MemoryStream(audio)))
            {
                double totalDurationInSeconds = reader.Length / (double)reader.WaveFormat.AverageBytesPerSecond;
                return (int)Math.Ceiling(totalDurationInSeconds / segmentDurationInSeconds);
            }
        }
        //public async Task<List<byte[]>> SplitAudio(byte[] audio, double segmentDurationInSeconds, int targetBitRate, CancellationToken cancellationToken)
        //{
        //    var audioSegments = new List<byte[]>();

        //    using (var reader = new Mp3FileReader(new MemoryStream(audio)))
        //    {
        //        int segmentNumber = 0;
        //        var buffer = new byte[reader.WaveFormat.AverageBytesPerSecond];
        //        while (reader.Position < reader.Length)
        //        {
        //            double segmentDurationInMilliseconds = segmentDurationInSeconds * 1000;
        //            using (var memoryStream = new MemoryStream())
        //            {
        //                using (var writer = new LameMP3FileWriter(memoryStream, reader.WaveFormat, targetBitRate))
        //                {
        //                    double writtenMilliseconds = 0;
        //                    while (writtenMilliseconds < segmentDurationInMilliseconds && reader.Position < reader.Length)
        //                    {
        //                        int bytesToRead = (int)Math.Min(buffer.Length, (segmentDurationInMilliseconds - writtenMilliseconds) / 1000.0 * reader.WaveFormat.AverageBytesPerSecond);
        //                        int bytesRead = await reader.ReadAsync(buffer, 0, bytesToRead, cancellationToken);
        //                        await writer.WriteAsync(buffer, 0, bytesRead, cancellationToken);
        //                        writtenMilliseconds += bytesRead / (double)reader.WaveFormat.AverageBytesPerSecond * 1000.0;
        //                    }
        //                }
        //                audioSegments.Add(memoryStream.ToArray());
        //            }
        //            segmentNumber++;
        //        }
        //    }

        //    return audioSegments;
        //}
    }
}
