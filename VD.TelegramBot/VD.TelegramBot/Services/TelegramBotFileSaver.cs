using Infrastructure.Firebase.Storage;
using TelegramBot.Interfaces;

namespace VD.TelegramBot.Services
{
    public class TelegramBotFileSaver : ITelegramBotFileSaver
    {
        private readonly IGoogleStorageAccessor _googleStorageAccessor;

        public TelegramBotFileSaver(IGoogleStorageAccessor googleStorageAccessor)
        {
            _googleStorageAccessor = googleStorageAccessor;
        }

        public async Task SaveFileAsync(byte[] file, long chatId, string fileExtension, GoogleStorageFileTypes googleStorageFileType, CancellationToken cancellationToken)
        {
            await using var stream = new MemoryStream(file);
            var datetime = DateTime.UtcNow;
            var filename = $"{chatId}/{googleStorageFileType}/{datetime.Year}_{datetime.Month}_{datetime.Day}_{datetime.Hour}_{datetime.Minute}_{datetime.Second}_{datetime.Millisecond}{fileExtension}";
            await _googleStorageAccessor.UploadFileAsync(filename, stream, cancellationToken);
        }
    }
}
