using Microsoft.SemanticKernel.Orchestration;

namespace CognitiveServices.AI.Services.SemanticKernel.Plugins;

public abstract class BasePlugin
{
    public BasePlugin()
    {

    }

    protected void SetAbortContextVariable(SKContext context)
    {
        if (!context.Variables.TryGetValue(SKContextVariableName.AbortPlan.ToString(), out _))
        {
            context.Variables.Set(SKContextVariableName.AbortPlan.ToString(), "True");
        }
    }

    protected async Task<string> ExecuteWithAbortCheckAsync(SKContext context, Func<Task<string>> action)
    {
        if (context.Variables.TryGetValue(SKContextVariableName.AbortPlan.ToString(), out _))
        {
            return string.Empty;
        }
        return await action();
    }

    protected string GetGoalFromContext(SKContext context)
    {
        if (context.Variables.TryGetValue(SKContextVariableName.Goal.ToString(), out var tgMessage))
        {
            return tgMessage;
        }
        return string.Empty;
    }

    protected long GetChatIdFromContext(SKContext context)
    {
        if (context.Variables.TryGetValue(SKContextVariableName.ChatId.ToString(), out var chatId))
        {
            return long.Parse(chatId);
        }
        return 0;
    }

    protected (string TelegramMessage, long ChatId) GetGoalAndChatIdFromContext(SKContext context)
    {
        string telegramMessage = string.Empty;
        long chatId = 0;

        if (context.Variables.TryGetValue(SKContextVariableName.Goal.ToString(), out var tgMessage))
        {
            telegramMessage = tgMessage;
        }

        if (context.Variables.TryGetValue(SKContextVariableName.ChatId.ToString(), out var chId))
        {
            chatId = long.Parse(chId);
        }

        return (telegramMessage, chatId);
    }
}
