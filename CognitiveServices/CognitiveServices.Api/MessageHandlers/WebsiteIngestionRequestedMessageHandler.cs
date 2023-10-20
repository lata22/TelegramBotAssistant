using Infrastructure.Messaging.Abstractions;
using Infrastructure.Messaging.Events;
using MassTransit;
using Microsoft.SemanticMemory;

namespace CognitiveServices.Api.MessageHandlers
{
    public class WebsiteIngestionRequestedMessageHandler : BaseMessageHandler<WebsiteIngestionRequested>
    {
        private readonly ISemanticMemoryClient _semanticMemoryClient;

        public WebsiteIngestionRequestedMessageHandler(ISemanticMemoryClient semanticMemoryClient)
        {
            _semanticMemoryClient = semanticMemoryClient;
        }

        public override async Task Consume(ConsumeContext<WebsiteIngestionRequested> context)
        {
            var documentId = "website_" + Guid.NewGuid().ToString();
            var tags = new TagCollection()
            {
                { "CreatedAd" , DateTime.UtcNow.ToString() },
                { nameof(context.Message.Url), context.Message.Url }
            };
            if (!string.IsNullOrWhiteSpace(context.Message.Title))
            {
                tags.Add(nameof(context.Message.Title), context.Message.Title);
            }

            await _semanticMemoryClient.ImportWebPageAsync(
                context.Message.Url,
                documentId,
                tags,
                context.Message.ChatId.ToString(),
                null,
                context.CancellationToken);

            await context.Publish(
                new DocumentProcessingStarted(
                    documentId,
                    context.Message.ChatId.ToString(),
                    context.Message.ChatId), context.CancellationToken);
        }
    }
}
