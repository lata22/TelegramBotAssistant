using Infrastructure.Messaging.Events;
using MassTransit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using System.ComponentModel;

namespace CognitiveServices.AI.Services.SemanticKernel.Plugins;

public class TelegramPlugin : BasePlugin
{
    private readonly IPublishEndpoint _publishEndpoint;

    public TelegramPlugin(
        IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    [SKFunction, Description("Sends a message via Telegram")]
    public async Task<string> SendTelegramMessageAsync(SKContext context, string message, CancellationToken cancellationToken)
    {
        return await ExecuteWithAbortCheckAsync(context, async () =>
        {
            long chatId = GetChatIdFromContext(context);
            await _publishEndpoint.Publish(new TelegramMessageCreated(chatId, message), cancellationToken);
            return string.Empty;
        });
    }

}