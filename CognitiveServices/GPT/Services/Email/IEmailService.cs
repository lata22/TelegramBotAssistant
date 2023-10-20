using MimeKit;

namespace CognitiveServices.AI.Services.Email
{
    public interface IEmailService
    {
        Task<UnreadEmails> GetUnreadEmails(int? top, int? skip, CancellationToken cancellationToken);
        Task<MimeMessage> GetEmailMessage(string userId, string messageId, CancellationToken cancellationToken);
        Task<string> SendEmailAsync(string[] recipientEmails, string subject, string body, CancellationToken cancellationToken);
    }
}
