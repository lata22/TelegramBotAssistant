using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Interfaces;

namespace VD.TelegramBot.Services
{
    public class TelegramBotCommandUpdaterHandler : ITelegramBotCommandUpdaterHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ITelegramBotConfigHandler _telegramBotConfigHandler;
        private readonly ILogger<TelegramBotCommandUpdaterHandler> _logger;
        public TelegramBotCommandUpdaterHandler(
            ITelegramBotClient botClient,
            ITelegramBotConfigHandler telegramBotConfigHandler,
            ILogger<TelegramBotCommandUpdaterHandler> logger)
        {
            _botClient = botClient;
            _telegramBotConfigHandler = telegramBotConfigHandler;
            _logger = logger;
        }

        public async Task UpdateCommands(CancellationToken cancellationToken)
        {
            try
            {
                var commands = await _telegramBotConfigHandler.ReadConfig(cancellationToken);
                var commandsToSet = new List<BotCommand>();
                commands.CommandList.ToList().ForEach(command =>
                {
                    if (!command.Contains("start"))
                    {
                        commandsToSet.Add(new BotCommand()
                        {
                            Command = command,
                            Description = "Temporary no description"
                        });
                    }
                });
                await _botClient.SetMyCommandsAsync(
                    commandsToSet,
                    null,
                    null,
                    cancellationToken);

                _logger.LogInformation("Bot commands updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
