using System.Diagnostics;
using System.Text;

namespace CognitiveServices.AI.Services.VideoAndAudioConverters
{
    public interface IAudioFormatConverter
    {
        Task<byte[]> ConvertOggToWav(byte[] oggAudioData);
    }

    public class AudioFormatConverter : IAudioFormatConverter
    {
        private readonly string _ffmpegBinFolderPath;

        public AudioFormatConverter(string ffmpegBinFolderPath)
        {
            _ffmpegBinFolderPath = ffmpegBinFolderPath;
        }

        private async Task<int> RunFFmpegAsync(string arguments)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegBinFolderPath + @"\ffmpeg.exe",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using Process process = new Process { StartInfo = processStartInfo };

            var stderrBuffer = new StringBuilder();
            var tcs = new TaskCompletionSource<int>();

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    stderrBuffer.AppendLine(e.Data);
                }
            };

            process.Exited += (sender, e) =>
            {
                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"FFmpeg error: {stderrBuffer}");
                    tcs.SetException(new Exception($"FFmpeg conversion failed. Exit code: {process.ExitCode}"));
                }
                else
                {
                    tcs.SetResult(process.ExitCode);
                }
            };

            process.EnableRaisingEvents = true;
            process.Start();

            process.BeginErrorReadLine();
            await process.StandardOutput.ReadToEndAsync();
            await tcs.Task;

            return process.ExitCode;
        }

        public async Task<byte[]> ConvertOggToWav(byte[] oggAudioData)
        {
            var inputOggFile = Path.GetTempFileName().Replace("tmp", "ogg");
            File.WriteAllBytes(inputOggFile, oggAudioData);
            var outputWavFile = Path.GetTempFileName().Replace("tmp", "wav");

            string arguments = $"-f ogg -i \"{inputOggFile}\" -ac 1 -ar 16000 -acodec pcm_s16le -f wav \"{outputWavFile}\"";
            await RunFFmpegAsync(arguments);

            var wavByteArray = await File.ReadAllBytesAsync(outputWavFile);
            File.Delete(inputOggFile);
            File.Delete(outputWavFile);
            return wavByteArray;
        }
    }
}
