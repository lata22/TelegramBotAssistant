using MediatR;
using TelegramBot.Interfaces;

namespace VD.TelegramBot.Queries
{
    public record StartQuery() : IRequest<string>;

    public class StartQueryHandler : IRequestHandler<StartQuery, string>
    {
        private readonly ITelegramBotConfigHandler _telegramBotConfigHandler;
        private readonly ILogger<StartQueryHandler> _logger;
        public StartQueryHandler(
            ITelegramBotConfigHandler telegramBotConfigHandler,
            ILogger<StartQueryHandler> logger)
        {
            _telegramBotConfigHandler = telegramBotConfigHandler;
            _logger = logger;
        }

        public async Task<string> Handle(StartQuery request, CancellationToken cancellationToken)
        {
            string result = string.Empty;
            try
            {
                var config = await _telegramBotConfigHandler.ReadConfig(cancellationToken);
                result = config.StartMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return result;
        }
    }
}
