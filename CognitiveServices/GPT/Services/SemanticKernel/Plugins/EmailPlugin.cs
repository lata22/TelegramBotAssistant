using CognitiveServices.AI.Services.Email;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using System.ComponentModel;

namespace CognitiveServices.AI.Services.SemanticKernel.Plugins;


public class EmailPlugin : BasePlugin
{
    private readonly ILogger<EmailPlugin> _logger;
    private readonly IGmailService _authenticatedGmailService;
    private readonly IHotmailService _hotmailService;
    private readonly IKernel _semanticKernel;
    public EmailPlugin(
        ILogger<EmailPlugin> logger,
        IGmailService authenticatedGmailService,
        IHotmailService hotmailService,
        IKernel semanticKernel)
    {
        _logger = logger;
        _authenticatedGmailService = authenticatedGmailService;
        _hotmailService = hotmailService;
        _semanticKernel = semanticKernel;
    }

    [SKFunction, Description("Sends an email via SMTP client from the given prompt")]
    public async Task<string> SendEmailAsync(
        SKContext context,
        [Description("Recipients of the email, separated by ',' or ';'.")] string emailRecipients,
        [Description("Subject of the email")] string emailSubject,
        [Description("Email content/body")] string emailBody,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(nameof(ExecuteWithAbortCheckAsync));
        return await ExecuteWithAbortCheckAsync(context, async () =>
        {
            string result = await _authenticatedGmailService.SendEmailAsync(
                emailRecipients.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries),
                emailSubject,
                emailBody,
                cancellationToken);
            _logger.LogInformation(result);
            return result;
        });
    }

    [SKFunction, Description("Get all unread emails from Gmail")]
    public async Task<string> GetUnreadGmailEmails(
        SKContext context,
        CancellationToken cancellationToken,
        [Description("Optional limit of the number of message to retrieve. Default is 10. Max is 20")] int? top = 10,
        [Description("Optional number of message to skip before retrieving results. Default is 0.")] int? skip = 0)
    {
        _logger.LogInformation(nameof(GetUnreadGmailEmails));
        return await ExecuteWithAbortCheckAsync(context, async () =>
        {
            var emailList = await _authenticatedGmailService.GetUnreadEmails(top, skip, cancellationToken);
            if (emailList.MimeMessages.Count == 0)
            {
                return "No tienes emails sin leer";
            }
            var json = emailList.ToJsonSummary();
            string? response = (await GetNiceSummarizationJsonEmailsResponse(context, json, cancellationToken)).GetValue<string>();
            if (response != null)
            {
                return response;
            }
            SetAbortContextVariable(context);
            return string.Empty;
        });
    }

    [SKFunction, Description("Get all unread emails from Hotmail")]
    public async Task<string> GetUnreadHotmailEmails(
        SKContext context,
        CancellationToken cancellationToken,
        [Description("Optional limit of the number of message to retrieve. Default is 10. Max is 20")] int? top = 10,
        [Description("Optional number of message to skip before retrieving results. Default is 0")] int? skip = 0)
    {
        _logger.LogInformation(nameof(GetUnreadHotmailEmails));
        return await ExecuteWithAbortCheckAsync(context, async () =>
        {
            var emailList = await _hotmailService.GetUnreadEmails(20, skip, cancellationToken);
            if (emailList.MimeMessages.Count == 0)
            {
                return "No tienes emails sin leer";
            }
            var json = emailList.ToJsonSummary();
            string? response = (await GetNiceSummarizationJsonEmailsResponse(context, json, cancellationToken)).GetValue<string>();
            if(response != null)
            {
                return response;
            }
            SetAbortContextVariable(context);
            return string.Empty;
        });
    }

    private async Task<FunctionResult> GetNiceSummarizationJsonEmailsResponse(SKContext context, string jsonEmails, CancellationToken cancellationToken)
    {
        var niceSummarizationMessage = _semanticKernel.CreateSemanticFunction(
                SemanticFunctions.NiceSummarizationJsonEmailsPrompt,
                nameof(SemanticFunctions.NiceSummarizationJsonEmailsPrompt),
                nameof(EmailPlugin),
                string.Empty);
        context.Variables.Add($"{nameof(jsonEmails)}", jsonEmails);
        return (await niceSummarizationMessage.InvokeAsync(context, null, cancellationToken));
    }

}
