using Infrastructure.Messaging.Events;
using MassTransit;
using System.Text.RegularExpressions;
using TelegramBot.Interfaces;

namespace VD.TelegramBot.Services
{
    public class WebsiteMessageHandler : ITelegramMessageTextHandler
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private string? _websiteTitle = null;
        public WebsiteMessageHandler(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task<bool> CanHandle(string message)
        {
            if (IsYouTubeUrl(message))
            {
                return false;
            }

            string wellFormedUrl = EnsureHttpScheme(message);
            if (!IsWellFormedUrl(wellFormedUrl, out Uri uriResult))
            {
                return false;
            }

            return await IsReachableAsync(uriResult).ConfigureAwait(false); ;
        }


        private bool IsYouTubeUrl(string message)
        {
            Regex regex = new Regex(@"https:\/\/(www\.youtube\.com|youtu\.be|youtube\.com)\/.*", RegexOptions.IgnoreCase);
            return regex.IsMatch(message);
        }

        private string EnsureHttpScheme(string message)
        {
            if (!message.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !message.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return "http://" + message;
            }
            return message;
        }

        private bool IsWellFormedUrl(string url, out Uri uriResult)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private async Task<bool> IsReachableAsync(Uri uri)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    bool isReachable = false;
                    HttpResponseMessage response = await httpClient.GetAsync(uri).ConfigureAwait(false); 
                    if(response.IsSuccessStatusCode)
                    {
                        isReachable = true;
                        var jsonContent = await response.Content.ReadAsStringAsync();
                        Regex titleTagRegex = new Regex(@"<title.*?>(.*?)<\/title>", RegexOptions.IgnoreCase);
                        Match titleTagMatch = titleTagRegex.Match(jsonContent);
                        if (titleTagMatch.Success)
                        {
                            _websiteTitle = titleTagMatch.Groups[1].Value;
                        }
                    }
                    return isReachable;
                }
                catch (HttpRequestException)
                {
                    return false;
                }
            }
        }

        public async Task Handle(string message, long chatId, CancellationToken cancellationToken)
        {
            await _publishEndpoint.Publish(new WebsiteIngestionRequested(_websiteTitle ?? string.Empty, message, chatId), cancellationToken);
        }

    }
}
