using Infrastructure.Firebase.Storage;

namespace TelegramBot.Interfaces
{
    public interface ITelegramBotFileSaver : ITelegramBotSingleton
    {
        Task SaveFileAsync(byte[] file, long chatId, string fileExtension, GoogleStorageFileTypes googleStorageFileType, CancellationToken cancellationToken);
    }
}
