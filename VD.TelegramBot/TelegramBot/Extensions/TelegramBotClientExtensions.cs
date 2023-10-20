using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Extensions
{
    public static class TelegramBotClientExtensions
    {
        public static async Task<Message> SendTextAsync(this ITelegramBotClient botClient, long chatId, string text, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(
                chatId,
                text,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                cancellationToken);
        }

        public static async Task<Message> SendPhotoAsync(this ITelegramBotClient botClient, long chatId, byte[] image, CancellationToken cancellationToken)
        {
            return await botClient.SendPhotoAsync(
                chatId,
                new Telegram.Bot.Types.InputFiles.InputOnlineFile(new MemoryStream(image), "image"),
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                cancellationToken);
        }

        public static async Task<Message> SendVideoAsync(this ITelegramBotClient botClient, ChatId chatId, byte[] video, CancellationToken cancellationToken)
        {
            return await botClient.SendVideoAsync(
                chatId,
                new Telegram.Bot.Types.InputFiles.InputOnlineFile(new MemoryStream(video), "video"),
                null,
                null,
                null,
                null, 
                null, 
                null, 
                null, 
                null, 
                null, 
                null, 
                null, 
                null, 
                null,cancellationToken);
        }
    }
}
