using CognitiveServices.AI.Services.SemanticKernel.Plugins;
using Infrastructure.Messaging.Events;
using MassTransit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Plugins.Core;
using System.Globalization;

namespace CognitiveServices.AI.Services.SemanticKernel
{
    public interface ISequentialPlannerFactory
    {
        Task<Plan> CreateSequentialPlan(long chatId, string goal, CancellationToken cancellationToken);
        Task<FunctionResult> ExecuteSequentialPlan(Plan plan, long chatId, string goal, CancellationToken cancellationToken);
    }

    public class SequentialPlannerFactory : ISequentialPlannerFactory
    {
        private readonly IKernel _semanticKernel;
        private readonly IPublishEndpoint _publishEndpoint;
        public SequentialPlannerFactory(
            IEnumerable<BasePlugin> plugins,
            TimePlugin timeSkill,
            SemanticKernelFactory semanticKernelFactory,
            IPublishEndpoint publishEndpoint)
        {
            _semanticKernel = semanticKernelFactory.GetKernelWithSmartGPTModel();
            foreach (var skill in plugins)
            {
                _semanticKernel.ImportFunctions(skill);
            }
            _semanticKernel.ImportFunctions(timeSkill);
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Plan> CreateSequentialPlan(long chatId, string goal, CancellationToken cancellationToken)
        {
            var planner = new SequentialPlanner(_semanticKernel);
            await SendTelegramNotification(chatId, "Recibido! Creando plan de ejecucion", cancellationToken);
            return await planner.CreatePlanAsync(goal, cancellationToken);
        }

        public async Task<FunctionResult> ExecuteSequentialPlan(Plan plan, long chatId, string goal, CancellationToken cancellationToken)
        {
            var context = CreateContext(chatId, goal);
            await SendTelegramNotification(chatId, $"Ejecutando plan:\n{string.Join('\n', plan.Steps.Select(s => "-" + s.Name))}", cancellationToken);
            return await plan.InvokeAsync(context, null, cancellationToken);
        }

        private SKContext CreateContext(long chatId, string goal)
        {
            var context = _semanticKernel.CreateNewContext();
            context.Variables.Set(SKContextVariableName.Today.ToString(), DateTimeOffset.Now.ToString(CultureInfo.CurrentCulture));
            context.Variables.Set(SKContextVariableName.ChatId.ToString(), chatId.ToString(CultureInfo.CurrentCulture));
            context.Variables.Set(SKContextVariableName.Index.ToString(), chatId.ToString(CultureInfo.CurrentCulture));
            context.Variables.Set(SKContextVariableName.Goal.ToString(), goal);
            return context;
        }

        private async Task SendTelegramNotification(long chatId, string message, CancellationToken cancellationToken)
        {
            await _publishEndpoint.Publish(new TelegramMessageCreated(
                chatId, message), cancellationToken);
        }
    }
}
