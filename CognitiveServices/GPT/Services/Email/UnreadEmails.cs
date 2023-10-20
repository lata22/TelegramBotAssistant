using MimeKit;
using Newtonsoft.Json;

namespace CognitiveServices.AI.Services.Email;

public record UnreadEmails(List<MimeMessage> MimeMessages, int UnreadEmailsCount);

public static class UnreadEmailsExtensions
{
    public static string ToJsonSummary(this UnreadEmails unreadEmails)
    {
        return JsonConvert.SerializeObject(new
        {
            Messages = unreadEmails.MimeMessages.Select(m => new
            {
                From = m.From.Select(f => f.ToString().Substring(f.ToString().IndexOf("<") + 1, f.ToString().LastIndexOf(">") - 1 - f.ToString().IndexOf("<"))),
                m.Subject,
                AttachmentsCount = m.Attachments.Count(),
                m.Date
            }).OrderByDescending(m => m.Date),
            TotalUnreadEmails = unreadEmails.UnreadEmailsCount
        }, Formatting.Indented);
    }
}