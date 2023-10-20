using Infrastructure.Messaging.Abstractions;
using Infrastructure.Messaging.Events;
using MassTransit;
using MediatR;
using VD.TelegramBot.Commands;

namespace VD.TelegramBot.MessageHandlers
{
    public class YoutubeTranscriptionCompletedMessageHandler : BaseMessageHandler<YoutubeTranscriptionCompleted>
    {
        private readonly IMediator _mediator;

        public YoutubeTranscriptionCompletedMessageHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override async Task Consume(ConsumeContext<YoutubeTranscriptionCompleted> context)
        {
            if (!string.IsNullOrEmpty(context.Message.Transcription))
            {
                await _mediator
                    .Send(new TelegramBotMessageNotificationCommand(
                            context.Message.Transcription,
                            context.Message.ChatId),
                        context.CancellationToken);
            }
        }
    }
}
