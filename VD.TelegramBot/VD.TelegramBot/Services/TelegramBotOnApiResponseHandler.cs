using Telegram.Bot;
using Telegram.Bot.Args;
using TelegramBot.Interfaces;

namespace VD.TelegramBot.Services
{

    public class TelegramBotOnApiResponseHandler : ITelegramBotOnApiResponseHandler
    {
        public TelegramBotOnApiResponseHandler()
        {
        }

        public async ValueTask HandleApiResponse(ITelegramBotClient botClient, ApiResponseEventArgs args, CancellationToken cancellationToken = default)
        {
            //    if (args.ApiRequestEventArgs.HttpRequestMessage.Conte.ToLower() == "getupdates")
            //    {
            //        var content = await args.ResponseMessage.Content.ReadAsByteArrayAsync(cancellationToken);
            //    var jsonString = System.Text.Encoding.ASCII.GetString(content);
            //    if (jsonString.Contains(nameof(My_Chat_Member).ToLower()))
            //    {
            //        var result = JsonConvert.DeserializeObject<TelegramResponseFromAction>(jsonString);
            //    }
            //    else
            //    {
            //        var result = JsonConvert.DeserializeObject<TelegramResponseFromCommand>(jsonString);
            //    }
            //}
            await ValueTask.CompletedTask;
        }
    }
}
