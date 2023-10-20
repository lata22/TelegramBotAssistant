using Newtonsoft.Json;
using TelegramBot.Config;
using TelegramBot.Interfaces;

namespace VD.TelegramBot.Services
{
    public class TelegramBotConfigHandler : ITelegramBotConfigHandler
    {
        private const string _telegramBotConfig = "TelegramBotConfig.json";
        private readonly string _jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), _telegramBotConfig);
        private readonly ILogger<TelegramBotConfigHandler> _logger;

        public TelegramBotConfigHandler(ILogger<TelegramBotConfigHandler> logger)
        {
            _logger = logger;
        }

        public async Task<TelegramBotConfig> ReadConfig(CancellationToken cancellationToken, string jsonFilePath = "")
        {
            string filePath = string.IsNullOrEmpty(jsonFilePath) ? _jsonFilePath : jsonFilePath;
            filePath = ValidateFilePath(filePath);

            try
            {
                using var _streamReader = new StreamReader(filePath);
                string json = await _streamReader.ReadToEndAsync(cancellationToken);
                return JsonConvert.DeserializeObject<TelegramBotConfig>(json) ??
                    throw new InvalidOperationException("Failed to deserialize TelegramBotConfig.");
            }
            catch (Exception ex)
            {
                throw new IOException("An error occurred while reading the TelegramBotConfig.", ex);
            }
        }

        public async Task UpdateConfig(TelegramBotConfig newConfig, CancellationToken cancellationToken, string jsonFilePath = "")
        {
            string filePath = string.IsNullOrEmpty(jsonFilePath) ? _jsonFilePath : jsonFilePath;
            filePath = ValidateFilePath(filePath);

            try
            {
                using var _streamWriter = new StreamWriter(filePath);
                string json = JsonConvert.SerializeObject(newConfig);
                await _streamWriter.WriteAsync(json);
                _logger.LogInformation($"{_telegramBotConfig} updated successfully");
            }
            catch (Exception ex)
            {
                throw new IOException("An error occurred while updating the TelegramBotConfig.", ex);
            }
        }

        private string ValidateFilePath(string filePath)
        {
            if (File.Exists(filePath))
            {
                return filePath;
            }

            throw new FileNotFoundException($"TelegramBotConfig json file not found at location: {filePath}");
        }
    }

    //public class TelegramBotConfigHandler : ITelegramBotConfigHandler
    //{
    //    private string _jsonFilePath = Directory.GetCurrentDirectory() + "\\TelegramBotConfig.json";

    //    public TelegramBotConfigHandler()
    //    {
    //    }

    //    public async Task<TelegramBotConfig> ReadConfig(CancellationToken cancellationToken, string jsonFilePath = "")
    //    {
    //        var result = new TelegramBotConfig();
    //        try
    //        {
    //            var jsonPath = ValidateFilePath(_jsonFilePath);
    //            using var _streamReader = new StreamReader(jsonPath);
    //            string json = await _streamReader.ReadToEndAsync(cancellationToken);
    //            result = JsonConvert.DeserializeObject<TelegramBotConfig>(json);
    //            _streamReader.Close();
    //        }
    //        catch (Exception ex)
    //        {
    //            LogService.LogMessageWithColor(Console.ForegroundColor, ConsoleColor.Red, ex.Message, LogType.Error);
    //        }
    //        return result!;
    //    }

    //    public async Task<Unit> UpdateConfig(TelegramBotConfig newConfig, CancellationToken cancellationToken, string jsonFilePath = "")
    //    {
    //        try
    //        {
    //            var jsonPath = ValidateFilePath(_jsonFilePath);
    //            using var _streamWriter = new StreamWriter(jsonPath);
    //            string json = JsonConvert.SerializeObject(newConfig);
    //            await _streamWriter.WriteAsync(json);
    //            _streamWriter.Close();
    //        }
    //        catch (Exception ex)
    //        {
    //            LogService.LogMessageWithColor(Console.ForegroundColor, ConsoleColor.Red, ex.Message, LogType.Error);
    //        }
    //        return Unit.Value;
    //    }

    //    private string ValidateFilePath(string newFilePath)
    //    {
    //        string result;
    //        if (File.Exists(_jsonFilePath))
    //        {
    //            result = _jsonFilePath;
    //        }
    //        else if (File.Exists(newFilePath))
    //        {
    //            result = newFilePath;
    //        }
    //        else
    //        {
    //            throw new FileNotFoundException($"TelegramBotConfig json file not found at location:\n {_jsonFilePath}\nAnd\n{newFilePath}");
    //        }
    //        return result;
    //    }
    //}
}
