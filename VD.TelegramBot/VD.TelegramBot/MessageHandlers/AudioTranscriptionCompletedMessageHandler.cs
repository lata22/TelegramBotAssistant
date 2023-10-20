using MassTransit;
using MediatR;
using Infrastructure.Messaging.Abstractions;
using Infrastructure.Messaging.Events;
using VD.TelegramBot.Commands;

namespace VD.TelegramBot.MessageHandlers
{
    public class AudioTranscriptionCompletedMessageHandler : BaseMessageHandler<AudioTranscriptionCompleted>
    {
        private readonly IMediator _mediator;
        public AudioTranscriptionCompletedMessageHandler(IMediator mediator)
        {
            _mediator = mediator;
        }
        public override async Task Consume(ConsumeContext<AudioTranscriptionCompleted> context)
        {
            if (!string.IsNullOrEmpty(context.Message.Text))
            {
                await _mediator.Send(new TelegramBotMessageNotificationCommand(context.Message.Text, context.Message.ChatId), context.CancellationToken);
            }
        }
    }
}
