using CognitiveServices.Db;
using Infrastructure.Messaging.Abstractions;
using Infrastructure.Messaging.Events;
using MassTransit;
using Microsoft.SemanticMemory;

namespace CognitiveServices.Api.MessageHandlers;

public class AudioTranscriptionIngestionRequestedMessageHandler : BaseMessageHandler<AudioTranscriptionIngestionRequested>
{
    private readonly ISemanticMemoryClient _semanticMemoryClient;
    private readonly ApplicationDBContext _dbContext;

    public AudioTranscriptionIngestionRequestedMessageHandler(
        ISemanticMemoryClient semanticMemoryClient, 
        ApplicationDBContext dbContext)
    {
        _semanticMemoryClient = semanticMemoryClient;
        _dbContext = dbContext;
    }

    public override async Task Consume(ConsumeContext<AudioTranscriptionIngestionRequested> context)
    {
        var audioTranscription = await _dbContext.AudioTranscriptions.FindAsync(context.Message.AudioTranscriptionId, context.CancellationToken);
        if(audioTranscription is not null)
        {
            var documentId = Guid.NewGuid().ToString();
            var a = await _semanticMemoryClient.ImportTextAsync(
                audioTranscription.Transcription,
                documentId,
                new TagCollection()
                {
                    {nameof(audioTranscription.FileName), audioTranscription.FileName },
                    {"CreatedAd" , DateTime.UtcNow.ToString() },
                },
                audioTranscription.TelegramChatId.ToString(),
                null,
                context.CancellationToken);
            await context.Publish(new AudioTranscriptionDocumentUpdated(audioTranscription.Id, documentId), context.CancellationToken);
        }
 
    }
}

