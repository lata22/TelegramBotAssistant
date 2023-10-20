using OpenAI.ObjectModels.ResponseModels;

namespace CognitiveServices.AI.Services.Whisper
{
    public interface IWhisperService
    {
        Task<List<AudioCreateTranscriptionResponse>> CreateTranscription(List<byte[]> audios, string language, CancellationToken cancellationToken);
        Task<AudioCreateTranscriptionResponse> CreateTranscription(byte[] audio, string fileName, string language, CancellationToken cancellationToken);
    }
}
