using Infrastructure.Messaging.Abstractions;
using Infrastructure.Messaging.Events;
using MassTransit;
using MediatR;
using VD.TelegramBot.Commands;

namespace VD.TelegramBot.MessageHandlers
{
    public class TelegramMessageCreatedMessageHandler : BaseMessageHandler<TelegramMessageCreated>
    {
        private readonly IMediator _mediator;

        public TelegramMessageCreatedMessageHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override async Task Consume(ConsumeContext<TelegramMessageCreated> context)
        {
            if (!string.IsNullOrEmpty(context.Message.Message))
            {
                await _mediator.Send(new TelegramBotMessageNotificationCommand(context.Message.Message, context.Message.ChatId), context.CancellationToken);
            }
        }
    }
}
