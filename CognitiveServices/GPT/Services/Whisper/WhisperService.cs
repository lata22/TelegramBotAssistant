using Microsoft.Extensions.Logging;
using OpenAI.Interfaces;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;

namespace CognitiveServices.AI.Services.Whisper;
public class WhisperService : IWhisperService
{
    private readonly IOpenAIService _openAIService;
    private readonly SemaphoreSlim _semaphore;
    private readonly ILogger<WhisperService> _logger;
    public WhisperService(
        IOpenAIService openAIService,
        ILogger<WhisperService> logger,
        int maxConcurrentCalls = 5)
    {
        _openAIService = openAIService;
        _semaphore = new SemaphoreSlim(maxConcurrentCalls);
        _logger = logger;
    }

    public async Task<AudioCreateTranscriptionResponse> CreateTranscription(byte[] audio, string fileName, string language, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);  // Wait until a spot is available

        try
        {
            var audioService = _openAIService.Audio;
            var request = new AudioCreateTranscriptionRequest()
            {
                ResponseFormat = "text",
                Language = language,
                Model = Models.WhisperV1,
                File = audio,
                FileName = Guid.NewGuid().ToString() + ".mp4"
            };
            var response = await audioService.CreateTranscription(request, cancellationToken);
            if (response.Error != null)
            {
                throw new Exception(string.Join('\n', response.Error.Messages));
            }
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
        finally
        {
            _semaphore.Release();
        }
        return new AudioCreateTranscriptionResponse();
    }

    public async Task<List<AudioCreateTranscriptionResponse>> CreateTranscription(List<byte[]> audios, string language, CancellationToken cancellationToken)
    {
        var tasks = audios.Select(async audio =>
        {
            var transcript = await CreateTranscription(audio, string.Empty, language, cancellationToken);
            return transcript;
        }).ToArray();

        return (await Task.WhenAll(tasks)).ToList();
    }
}
