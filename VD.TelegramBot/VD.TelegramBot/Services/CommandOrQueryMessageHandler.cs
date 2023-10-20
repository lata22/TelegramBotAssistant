using Telegram.Bot;
using TelegramBot.Extensions;
using TelegramBot.Interfaces;

namespace VD.TelegramBot.Services
{
    public class CommandOrQueryMessageHandler : ITelegramMessageTextHandler
    {
        private readonly ITelegramBotQueryDispatcher _telegramBotQueryDispatcher;
        private readonly ITelegramBotCommandTrigger _telegramBotCommandTrigger;
        private readonly ITelegramBotClient _botClient;
        public CommandOrQueryMessageHandler(
            ITelegramBotQueryDispatcher telegramBotQueryDispatcher,
            ITelegramBotCommandTrigger telegramBotCommandTrigger,
            ITelegramBotClient botClient)
        {
            _telegramBotQueryDispatcher = telegramBotQueryDispatcher;
            _telegramBotCommandTrigger = telegramBotCommandTrigger;
            _botClient = botClient;
        }

        public async Task Handle(string message, long chatId, CancellationToken cancellationToken)
        {
            string result = string.Empty;
            if (message.ToLower().Contains("get"))
            {
                result = await _telegramBotQueryDispatcher.DispatchQuery(message, cancellationToken);
            }
            else
            {
                await _telegramBotCommandTrigger.TriggerCommand(message, cancellationToken);
            }
            if (!string.IsNullOrWhiteSpace(result))
            {
                await _botClient.SendTextAsync(chatId, result, cancellationToken);
            }
        }

        public Task<bool> CanHandle(string message)
        {
            return Task.FromResult(message.ToLower().StartsWith("/") && message.Length > 4);
        }
    }
}
