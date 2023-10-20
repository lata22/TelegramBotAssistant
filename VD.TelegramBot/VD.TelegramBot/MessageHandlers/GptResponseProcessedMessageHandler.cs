using Infrastructure.Messaging.Abstractions;
using Infrastructure.Messaging.Events;
using MassTransit;
using MediatR;
using VD.TelegramBot.Commands;

namespace VD.TelegramBot.MessageHandlers
{
    public class GptResponseProcessedMessageHandler : BaseMessageHandler<GptResponseProcessed>
    {
        private readonly IMediator _mediator;

        public GptResponseProcessedMessageHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override async Task Consume(ConsumeContext<GptResponseProcessed> context)
        {
            if (!string.IsNullOrEmpty(context.Message.Text))
            {
                await _mediator.Send(new TelegramBotMessageNotificationCommand(context.Message.Text, context.Message.ChatId), context.CancellationToken);
            }
        }
    }
}
