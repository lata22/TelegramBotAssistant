using Infrastructure.Messaging.Events;
using MassTransit;
using MediatR;
using TelegramBot.Interfaces;
using VD.TelegramBot.Api.Extensions;

namespace VD.TelegramBot.Services
{
    public class TelegramBotMessageHandler : ITelegramBotMessageHandler
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IEnumerable<ITelegramMessageTextHandler> _messageTextHandlers;
        public TelegramBotMessageHandler(
            IPublishEndpoint publishEndpoint, 
            IEnumerable<ITelegramMessageTextHandler> messageTextHandlers
            )
        {
            _publishEndpoint = publishEndpoint;

            _messageTextHandlers = messageTextHandlers;
        }


        public async Task<Unit> HandleMessage(Telegram.Bot.Types.Message message, CancellationToken cancellationToken)
        {
            string returnMessage = string.Empty;
            if (!string.IsNullOrWhiteSpace(message.Text))
            {
                var messageHandler = await _messageTextHandlers.FirstOrDefaultAsync(async mh => await mh.CanHandle(message.Text));
                if (messageHandler != null)
                {
                    await messageHandler.Handle(message.Text, message.Chat.Id, cancellationToken);
                }
                else
                {
                    await HandleGptRequest(message.Chat.Id, message.Text, cancellationToken);
                }
            }
            return Unit.Value;
        }

        private async Task HandleGptRequest(long chatId, string text, CancellationToken cancellationToken)
        {
            await _publishEndpoint.Publish(new GptResponseRequested(chatId, text), cancellationToken);
        }
    }
}
