using MediatR;
using System.Reflection;
using TelegramBot.Interfaces;

namespace VD.TelegramBot.Services
{
    public class TelegramBotCommandTrigger : ITelegramBotCommandTrigger
    {
        private readonly IMediator _mediator;
        private readonly ITelegramBotConfigHandler _telegramBotConfigHandler;
        private readonly ILogger<TelegramBotCommandTrigger> _logger;
        public TelegramBotCommandTrigger(
            ITelegramBotConfigHandler telegramBotConfigHandler,
            IMediator mediator,
            ILogger<TelegramBotCommandTrigger> logger)
        {
            _telegramBotConfigHandler = telegramBotConfigHandler;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task TriggerCommand(string command, CancellationToken cancellationToken)
        {
            var telegramConfig = await _telegramBotConfigHandler.ReadConfig(
                cancellationToken,
                Directory.GetCurrentDirectory() + "/TelegramBotConfig.json");
            if (telegramConfig.CommandList.Any(cmd => cmd == command))
            {
                try
                {
                    command = command.Replace("/", "");
                    var typeToDispatch = Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .FirstOrDefault(t => t.Name.ToLower().Replace("command", "") == command);
                    var instance = Activator.CreateInstance(typeToDispatch!);
                    await _mediator.Send(instance!, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }
    }
}
