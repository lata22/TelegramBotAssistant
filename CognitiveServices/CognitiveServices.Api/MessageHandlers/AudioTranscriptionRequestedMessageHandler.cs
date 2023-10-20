
using CognitiveServices.AI.Services.Whisper;
using Infrastructure.Messaging.Abstractions;
using Infrastructure.Messaging.Events;
using Infrastructure.Redis.CacheService;
using MassTransit;

namespace CognitiveServices.Api.MessageHandlers;
public class AudioTranscriptionRequestedMessageHandler : BaseMessageHandler<AudioTranscriptionRequested>
{
    private readonly IWhisperService _whisperService;
    private readonly ICacheReaderService<byte[]> _cacheReaderService;
    public AudioTranscriptionRequestedMessageHandler(
        IWhisperService whisperService,
        ICacheReaderService<byte[]> cacheReaderService)
    {
        _whisperService = whisperService;
        _cacheReaderService = cacheReaderService;
    }

    public override async Task Consume(ConsumeContext<AudioTranscriptionRequested> context)
    {
        var audioBytes = await _cacheReaderService.GetCachedValueAsync(context.Message.CacheKey);
        if (audioBytes != null)
        {
            var whisperTranscription = await _whisperService.CreateTranscription(
                audioBytes,
                context.Message.CacheKey,
                "es",
                context.CancellationToken);
            if (!string.IsNullOrWhiteSpace(whisperTranscription.Text))
            {
                await Task.WhenAll(
                    context.Publish(new TelegramMessageCreated(context.Message.TelegramChatId, whisperTranscription.Text)),
                    context.Publish(new AudioTranscriptionCreated(whisperTranscription.Text, context.Message.TelegramChatId, string.Empty)));
            }
        }
    }
}

