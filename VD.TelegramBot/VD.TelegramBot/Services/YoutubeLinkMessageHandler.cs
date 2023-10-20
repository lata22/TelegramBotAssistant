using Infrastructure.Messaging.Events;
using MassTransit;
using TelegramBot.Interfaces;

namespace VD.TelegramBot.Services
{
    public class YoutubeLinkMessageHandler : ITelegramMessageTextHandler
    {
        private readonly IPublishEndpoint _publishEndpoint;
        public YoutubeLinkMessageHandler(
            IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Handle(string message, long chatId, CancellationToken cancellationToken)
        {
            await _publishEndpoint.Publish(new YoutubeVideoDownloadRequested(Guid.NewGuid(), message, chatId), cancellationToken);
        }

        public Task<bool> CanHandle(string message)
        {
            return Task.FromResult(message.ToLower().Contains("https://youtube.com") ||
               message.ToLower().Contains("https://youtu.be") ||
               message.ToLower().Contains("https://www.youtube.com"));
        }
    }
}
