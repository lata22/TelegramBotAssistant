using FFMpegCore;

namespace CognitiveServices.AI.Services.AudioSplitters
{
    public interface IMP4AudioSplitter
    {
        Task<List<byte[]>> SplitMP4Audio(byte[] mp4Audio, CancellationToken cancellationToken);
    }

    public class MP4AudioSplitter : IMP4AudioSplitter
    {
        private const int MaxSegmentSize = 25 * 1024 * 1024; // 25 MB in bytes
        private readonly string _ffmpegBinFolderPath;

        public MP4AudioSplitter(string ffmpegBinFolderPath)
        {
            _ffmpegBinFolderPath = ffmpegBinFolderPath;
        }

        public async Task<List<byte[]>> SplitMP4Audio(byte[] mp4File, CancellationToken cancellationToken)
        {
            List<byte[]> audioSegments = new List<byte[]>();

            // Define a temporary directory to store the split audio files
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            var inputFile = Path.Combine(tempDir, "input.mp4");
            var outputFilePattern = Path.Combine(tempDir, "output_segment_%03d.mp4");

            await File.WriteAllBytesAsync(inputFile, mp4File, cancellationToken);

            // Run FFMpeg to split the audio
            await FFMpegArguments
                .FromFileInput(inputFile)
                .OutputToFile(outputFilePattern, false, options => options
                    .ForceFormat("segment") // Enable segmentation
                   .WithCustomArgument("-acodec copy") // Copy audio stream
                   .WithCustomArgument($"-fs {MaxSegmentSize}") // 25 MB limit per segment
                   .WithCustomArgument("-segment_time 1000")
                   .WithCustomArgument("-reset_timestamps 1"))// Reset timestamps
               .Configure(options =>
                {
                    options.BinaryFolder = _ffmpegBinFolderPath;
                    options.LogLevel = FFMpegCore.Enums.FFMpegLogLevel.Verbose;
                })
                .ProcessAsynchronously(true, null);

            // Read split files back into byte arrays
            var segmentFiles = Directory.GetFiles(tempDir, "output_segment_*.mp4");
            foreach (var segmentFile in segmentFiles)
            {
                var segmentData = await File.ReadAllBytesAsync(segmentFile, cancellationToken);
                audioSegments.Add(segmentData);
            }

            // Cleanup temporary files and directory
            Directory.Delete(tempDir, true);

            return audioSegments;
        }
    }
}
