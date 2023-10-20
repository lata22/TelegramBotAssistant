using Infrastructure.Messaging.Abstractions;
using Infrastructure.Messaging.Events;
using MassTransit;
using Microsoft.SemanticMemory;

namespace CognitiveServices.Api.MessageHandlers;

public class DocumentProcessingStartedMessageHandler : BaseMessageHandler<DocumentProcessingStarted>
{
    private readonly ISemanticMemoryClient _semanticMemoryClient;
    private readonly IPublishEndpoint _publishEndpoint;

    public DocumentProcessingStartedMessageHandler(
        ISemanticMemoryClient semanticMemoryClient,
        IPublishEndpoint publishEndpoint)
    {
        _semanticMemoryClient = semanticMemoryClient;
        _publishEndpoint = publishEndpoint;
    }

    public override async Task Consume(ConsumeContext<DocumentProcessingStarted> context)
    {
        var isReady = await _semanticMemoryClient.GetDocumentStatusAsync(
            context.Message.DocumentId,
            context.Message.Index,
            context.CancellationToken);
        
        if (isReady != null && isReady.Completed)
        {
            await _publishEndpoint.Publish(
                new TelegramMessageCreated(
                    context.Message.ChatId,
                    $"Su archivo ha sido procesado con éxito y ya está disponible con el ID: {isReady.DocumentId}"),
                context.CancellationToken);
        }
        else
        {
            await Task.Delay(5000);
            await _publishEndpoint.Publish(context.Message, context.CancellationToken);
        }
    }
}

