using CognitiveServices.AI.Services.SemanticKernel;
using Infrastructure.Messaging.Abstractions;
using Infrastructure.Messaging.Events;
using MassTransit;

namespace CognitiveServices.Api.MessageHandlers
{
    public class GptResponseRequestedMessageHandler : BaseMessageHandler<GptResponseRequested>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ISequentialPlannerFactory _sequentialPlannerFactory;

        public GptResponseRequestedMessageHandler(
            IPublishEndpoint publishEndpoint,
            ISequentialPlannerFactory sequentialPlannerFactory)
        {
            _publishEndpoint = publishEndpoint;
            _sequentialPlannerFactory = sequentialPlannerFactory;
        }

        public override async Task Consume(ConsumeContext<GptResponseRequested> context)
        {
            string message;
            try
            {
                var plan = await _sequentialPlannerFactory
                    .CreateSequentialPlan(context.Message.ChatId, context.Message.Text, context.CancellationToken); 
                var planResult = await _sequentialPlannerFactory
                    .ExecuteSequentialPlan(
                    plan, 
                    context.Message.ChatId,
                    context.Message.Text,
                    context.CancellationToken);
                message = planResult.GetValue<string>() ?? "String Empty";
            }
            catch (Exception ex)
            {
                message = ex.InnerException == null ?
                    ex.Message :
                    ex.InnerException.Message + '\n' + ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
            {
                await _publishEndpoint.Publish(new GptResponseProcessed(context.Message.ChatId, message), context.CancellationToken);
            }
        }

    }
}
