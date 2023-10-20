using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Interfaces;

namespace VD.TelegramBot.Services
{
    public class TelegramBotUpdateHandler : ITelegramBotUpdateHandler
    {
        private readonly ITelegramBotMessageHandler _telegramBotMessageHandler;
        private readonly ITelegramBotAccountHandler _telegramBotAccountHandler;
        private readonly ITelegramBotPhotoMessageHandler _telegramBotPhotoMessageHandler;
        private readonly ITelegramBotVideoMessageHandler _telegramBotVideoMessageHandler;
        private readonly ITelegramBotAudioMessageHandler _telegramBotAudioMessageHandler;
        private readonly ITelegramBotDocumentMessageHandler _telegramBotDocumentMessageHandler;

        public TelegramBotUpdateHandler(
            ITelegramBotMessageHandler telegramBotMessageHandler,
            ITelegramBotAccountHandler telegramBotAccountHandler,
            ITelegramBotPhotoMessageHandler telegramBotPhotoMessageHandler,
            ITelegramBotVideoMessageHandler telegramBotVideoMessageHandler,
            ITelegramBotAudioMessageHandler telegramBotAudioMessageHandler,
            ITelegramBotDocumentMessageHandler telegramBotDocumentMessageHandler)
        {
            _telegramBotMessageHandler = telegramBotMessageHandler;
            _telegramBotAccountHandler = telegramBotAccountHandler;
            _telegramBotPhotoMessageHandler = telegramBotPhotoMessageHandler;
            _telegramBotVideoMessageHandler = telegramBotVideoMessageHandler;
            _telegramBotAudioMessageHandler = telegramBotAudioMessageHandler;
            _telegramBotDocumentMessageHandler = telegramBotDocumentMessageHandler;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not null)
            {
                switch (update.Message)
                {
                    case { Text: "/start" }:
                        await _telegramBotAccountHandler.Handle(update.Message, cancellationToken);
                        break;
                    case { Text.Length: > 0 }:
                        await _telegramBotMessageHandler.HandleMessage(update.Message, cancellationToken);
                        break;
                    case { Photo.Length: > 0 }:
                        await _telegramBotPhotoMessageHandler.HandlePhotoMessage(update.Message, cancellationToken);
                        break;
                    case { Video.FileSize: > 0 }:
                        await _telegramBotVideoMessageHandler.HandleVideoMessage(update.Message, cancellationToken);
                        break;
                    case { Voice.FileSize: > 0 }:
                        await _telegramBotAudioMessageHandler.HandleAudioMessage(update.Message, cancellationToken);
                        break;
                    case { Document.FileSize: > 0 }:
                        await _telegramBotDocumentMessageHandler.HandleDocumentMessage(update.Message, cancellationToken);
                        break;
                }
            }
        }
    }
}
