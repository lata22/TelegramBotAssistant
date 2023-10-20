using Infrastructure.Hotmail.Config;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace CognitiveServices.AI.Services.Email;
public class HotmailService : IHotmailService
{
    private readonly HotmailConfig _hotmailConfig;
    private readonly ILogger<HotmailService> _logger;
    public HotmailService(
        HotmailConfig hotmailConfig,
        ILogger<HotmailService> logger)
    {
        _hotmailConfig = hotmailConfig;
        _logger = logger;
    }

    public Task<MimeMessage> GetEmailMessage(string userId, string messageId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<UnreadEmails> GetUnreadEmails(int? top, int? skip, CancellationToken cancellationToken)
    {
        _logger.LogInformation(nameof(GetUnreadEmails));
        var result = new List<MimeMessage>();
        using (var client = new ImapClient())
        {
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await client.ConnectAsync(_hotmailConfig.ImapServer, _hotmailConfig.ImapPort, true, cancellationToken);
            await client.AuthenticateAsync(_hotmailConfig.SenderEmail, _hotmailConfig.Password, cancellationToken);
            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);
            var uids = await inbox.SearchAsync(SearchQuery.NotSeen, cancellationToken);
            uids.Skip(skip ?? 0)
                .Take(top ?? 10)
                .ToList()
                .ForEach(id =>
                {
                    result.Add(inbox.GetMessage(id, cancellationToken));
                });
            await client.DisconnectAsync(true, cancellationToken);
            return new UnreadEmails(result, uids.Count);
        }
    }

    public async Task<string> SendEmailAsync(string[] recipientEmails, string subject, string body, CancellationToken cancellationToken)
    {
        string result;
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_hotmailConfig.SenderName, _hotmailConfig.SenderEmail));
            foreach (var email in recipientEmails)
            {
                message.To.Add(new MailboxAddress("", email));
            }
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_hotmailConfig.ImapServer, _hotmailConfig.ImapPort, MailKit.Security.SecureSocketOptions.StartTls, cancellationToken);
                await client.AuthenticateAsync(_hotmailConfig.SenderEmail, _hotmailConfig.Password, cancellationToken);
                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);
            }
            result = "Email successfully sent.";
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

