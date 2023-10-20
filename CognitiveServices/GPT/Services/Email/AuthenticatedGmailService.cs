using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Infrastructure.Firebase.Config;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace CognitiveServices.AI.Services.Email
{
    public class AuthenticatedGmailService : IGmailService
    {
        string[] Scopes = { GmailService.Scope.GmailReadonly };
        string ApplicationName = "TelegramBotApp";
        private readonly GoogleConfig _googleConfig;
        private readonly ILogger<AuthenticatedGmailService> _logger;

        public AuthenticatedGmailService(
            GoogleConfig config,
            ILogger<AuthenticatedGmailService> logger)
        {
            _googleConfig = config;
            _logger = logger;
        }

        public async Task<UnreadEmails> GetUnreadEmails(int? top, int? skip, CancellationToken cancellationToken)
        {
            var result = new List<MimeMessage>();
            int unreadEmailCount = 0;
            var gmailService = await GetGmailService(cancellationToken);

            // Define parameters of request.
            UsersResource.MessagesResource.ListRequest request = gmailService.Users.Messages.List("me");
            request.Q = "is:unread";  // Fetch unread emails only

            // List messages.
            ListMessagesResponse response = await request.ExecuteAsync(cancellationToken);
            if (response.Messages != null && response.Messages.Count > 0)
            {
                unreadEmailCount = response.Messages.Count;
                var messages = response.Messages.Skip(skip ?? 0).Take(top ?? 10);
                var mimeMessages = await ConvertGoogleMessagesToMimeMessages(messages, cancellationToken);
                result.AddRange(mimeMessages);
            }
            else
            {
                _logger.LogWarning("No unread emails found.");
            }
            return new UnreadEmails(result, unreadEmailCount);
        }

        private async Task<MimeMessage[]> ConvertGoogleMessagesToMimeMessages(IEnumerable<Message> messages, CancellationToken cancellationToken)
        {
            // Use LINQ to create a collection of Tasks where each task converts a Message to a MimeMessage
            var mimeMessageTasks = messages.Select(async message =>
            {
                string base64Url = message.Raw.Replace('-', '+').Replace('_', '/');
                byte[] emailBytes = Convert.FromBase64String(base64Url);

                using (MemoryStream emailStream = new MemoryStream(emailBytes))
                {
                    return await MimeMessage.LoadAsync(emailStream, cancellationToken);
                }
            });

            // Wait for all the tasks to complete and collect their results into an array
            return await Task.WhenAll(mimeMessageTasks);
        }

        public async Task<MimeMessage> GetEmailMessage(string userId, string messageId, CancellationToken cancellationToken)
        {
            var gmailService = await GetGmailService(cancellationToken);
            var request = gmailService.Users.Messages.Get(userId, messageId);
            Message message = await request.ExecuteAsync(cancellationToken);
            var mimeMessage = await ConvertGoogleMessagesToMimeMessages(new List<Message>()
            {
                message
            }, cancellationToken);
            return mimeMessage.First();
        }

        private async Task<GmailService> GetGmailService(CancellationToken cancellationToken)
        {
            UserCredential credential;

            GoogleClientSecrets googleSecrets = await GoogleClientSecrets.FromFileAsync(_googleConfig.OAuthFilePath, cancellationToken);
            //string credPath = "token.json";
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                googleSecrets.Secrets,
                Scopes,
                _googleConfig.GmailEmailConfig.SenderEmail,
                cancellationToken
                //new FileDataStore(credPath, true)
                );

            return new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        public async Task<string> SendEmailAsync(string[] recipientEmails, string subject, string body, CancellationToken cancellationToken)
        {
            string result;
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_googleConfig.GmailEmailConfig.SenderName, _googleConfig.GmailEmailConfig.SenderEmail));

                foreach (var recipient in recipientEmails)
                {
                    message.To.Add(new MailboxAddress("", recipient));
                }
                message.Subject = subject;
                message.Body = new TextPart("plain") { Text = body };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_googleConfig.GmailEmailConfig.SMTPServer, _googleConfig.GmailEmailConfig.SMTPPort, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_googleConfig.GmailEmailConfig.SenderEmail, _googleConfig.GmailEmailConfig.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                result = "Email enviado!";
                _logger.LogInformation(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                result = $"An error occurred while sending the email: {ex.Message}";
            }
            return result;
        }
    }
}
