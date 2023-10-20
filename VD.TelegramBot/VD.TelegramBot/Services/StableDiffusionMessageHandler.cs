using Infrastructure.Messaging.Events;
using MassTransit;
using TelegramBot.Interfaces;

namespace VD.TelegramBot.Services
{
    public class StableDiffusionMessageHandler : ITelegramMessageTextHandler
    {
        private readonly IPublishEndpoint _publishEndpoint;
        public StableDiffusionMessageHandler(
            IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Handle(string message, long chatId, CancellationToken cancellationToken)
        {
            await _publishEndpoint.Publish(new SDImageCreationRequested(message, chatId), cancellationToken);
        }

        public Task<bool> CanHandle(string message)
        {
            return Task.FromResult(message.ToLower().StartsWith("sd "));
        }
    }
}
