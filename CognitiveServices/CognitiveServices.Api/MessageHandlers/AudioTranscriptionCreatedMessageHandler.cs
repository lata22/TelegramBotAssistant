using CognitiveServices.Db;
using CognitiveServices.Db.Entities;
using Infrastructure.Messaging.Abstractions;
using Infrastructure.Messaging.Events;
using MassTransit;

namespace CognitiveServices.Api.MessageHandlers;
public class AudioTranscriptionCreatedMessageHandler : BaseMessageHandler<AudioTranscriptionCreated>
{
    private readonly ApplicationDBContext _dbContext;

    public AudioTranscriptionCreatedMessageHandler(
        ApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task Consume(ConsumeContext<AudioTranscriptionCreated> context)
    {
        var audioTranscription = new AudioTranscription()
        {
            CreatedAt = DateTime.UtcNow,
            FileName = string.Empty,
            Summary = string.Empty,
            TelegramChatId = context.Message.TelegramChatId,
            Transcription = context.Message.Transcription,
            DocumentId = context.Message.DocumentId,
        };
        await _dbContext.AddAsync(audioTranscription, context.CancellationToken);
        await _dbContext.SaveChangesAsync(context.CancellationToken);
        await context.Publish(new AudioTranscriptionIngestionRequested(audioTranscription.Id), context.CancellationToken);
    }
}

