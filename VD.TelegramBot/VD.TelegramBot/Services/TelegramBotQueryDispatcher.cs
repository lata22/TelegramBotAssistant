using MediatR;
using System.Reflection;
using TelegramBot.Interfaces;
using VD.TelegramBot.Db.Extensions;

namespace VD.TelegramBot.Services
{
    public class TelegramBotQueryDispatcher : ITelegramBotQueryDispatcher
    {
        private readonly IMediator _mediator;
        private readonly ITelegramBotConfigHandler _telegramBotConfigHandler;
        private readonly ILogger<TelegramBotQueryDispatcher> _logger;
        public TelegramBotQueryDispatcher(
            IMediator mediator,
            ITelegramBotConfigHandler telegramBotConfigHandler,
            ILogger<TelegramBotQueryDispatcher> logger)
        {
            _mediator = mediator;
            _telegramBotConfigHandler = telegramBotConfigHandler;
            _logger = logger;
        }

        public async Task<string> DispatchQuery(string telegramCommand, CancellationToken cancellation)
        {
            var telegramConfig = await _telegramBotConfigHandler
                    .ReadConfig(cancellation, Directory.GetCurrentDirectory() + "/TelegramBotConfig.json");
            string message = telegramConfig.ErrorMessage;
            try
            {
                if (telegramConfig.CommandList.Any(cmd => cmd == telegramCommand))
                {
                    telegramCommand = telegramCommand.Remove(0, 1); //remove first char that is the "/"
                    var typeToDispatch = Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .FirstOrDefault(t => t.Name.ToLower().Replace("query", "") == telegramCommand);
                    var instance = Activator.CreateInstance(typeToDispatch!);
                    var result = await _mediator.Send(instance!, cancellation);
                    if (result is not null)
                    {
                        message = result switch
                        {
                            string s => s,
                            _ => result.GenericEntityToString()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return message;
        }
    }
}
