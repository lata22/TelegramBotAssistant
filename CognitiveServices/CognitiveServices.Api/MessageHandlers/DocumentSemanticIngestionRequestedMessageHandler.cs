using CognitiveServices.Db.Entities;
using Infrastructure.Messaging.Abstractions;
using Infrastructure.Messaging.Events;
using Infrastructure.Redis.CacheService;
using MassTransit;
using Microsoft.SemanticMemory;

namespace CognitiveServices.Api.MessageHandlers;

public class DocumentSemanticIngestionRequestedMessageHandler : BaseMessageHandler<DocumentSemanticIngestionRequested>
{
    private readonly ILogger<DocumentSemanticIngestionRequestedMessageHandler> _logger;
    private readonly ICacheReaderService<byte[]> _cacheReaderService;
    private readonly ISemanticMemoryClient _semanticMemoryClient;
    public DocumentSemanticIngestionRequestedMessageHandler(
        ILogger<DocumentSemanticIngestionRequestedMessageHandler> logger,
        ICacheReaderService<byte[]> cacheReaderService,
        ISemanticMemoryClient semanticMemoryClient)
    {
        _logger = logger;
        _cacheReaderService = cacheReaderService;
        _semanticMemoryClient = semanticMemoryClient;
    }

    public override async Task Consume(ConsumeContext<DocumentSemanticIngestionRequested> context)
    {
        byte[]? documentFile = await _cacheReaderService.GetCachedValueAsync(context.Message.CacheKey);
        if (documentFile == null)
        {
            _logger.LogError("Document file not found in cache");
            return;
        }
        var documentId = await _semanticMemoryClient.ImportDocumentAsync(
            new MemoryStream(documentFile),
            context.Message.FileName,
            Guid.NewGuid().ToString(),
            new TagCollection()
            {
                { nameof(context.Message.FileName),  context.Message.FileName},
                { nameof(DocumentIngestion.CreatedAt), DateTime.UtcNow.ToString()}
            },
            context.Message.TelegramChatid.ToString(),
            null, // We should investigate what are these Steps parameter
            context.CancellationToken);

        await Task.WhenAll(
            context.Publish(new DocumentIngestionCreated(documentId, context.Message.FileName, context.Message.TelegramChatid), context.CancellationToken),
            context.Publish(new TelegramMessageCreated(context.Message.TelegramChatid, "Documento recibido!\nTe notificaré cuando termine de procesarlo"), context.CancellationToken),
            context.Publish(new DocumentProcessingStarted(documentId, context.Message.TelegramChatid.ToString(), context.Message.TelegramChatid), context.CancellationToken));
    }
}
