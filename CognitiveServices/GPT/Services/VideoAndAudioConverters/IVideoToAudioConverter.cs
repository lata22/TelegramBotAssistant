using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;

namespace CognitiveServices.AI.Services.VideoAndAudioConverters
{
    public interface IVideoToAudioConverter
    {
        Task<byte[]> ConvertAndGetMP4ToMP3(byte[] audioFile, CancellationToken cancellationToken);
    }

    public class VideoToAudioConverter : IVideoToAudioConverter
    {
        private readonly string _ffmpegBinFolderPath;

        public VideoToAudioConverter(string ffmpegBinFolderPath)
        {
            _ffmpegBinFolderPath = ffmpegBinFolderPath;
        }

        public async Task<byte[]> ConvertAndGetMP4ToMP3(byte[] audioFile, CancellationToken cancellationToken)
        {
            var outputStream = new MemoryStream();
            await FFMpegArguments
                .FromPipeInput(new StreamPipeSource(new MemoryStream(audioFile)), options => options.WithVideoCodec("h264"))
                .OutputToPipe(new StreamPipeSink(outputStream), options =>
                    options.ForceFormat("mp3")
                    .WithAudioBitrate(AudioQuality.Normal))
                .Configure(options =>
                {
                    options.BinaryFolder = _ffmpegBinFolderPath;
                    options.LogLevel = FFMpegLogLevel.Verbose;
                })
                .ProcessAsynchronously(true, null);
            return outputStream.ToArray();
        }
    }
}
