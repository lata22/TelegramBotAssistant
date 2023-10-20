using DocumentFormat.OpenXml.Wordprocessing;
using Infrastructure.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using System.ComponentModel;

namespace CognitiveServices.AI.Services.SemanticKernel.Plugins;

public class StableDiffusionSkill : BasePlugin
{
    private readonly ILogger<StableDiffusionSkill> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    public StableDiffusionSkill(
        ILogger<StableDiffusionSkill> logger,
        IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    [SKFunction, Description("Generates or creates an image using Stable Diffusion given a user prompt")]
    public async Task GenerateStableDiffusionImage(
        SKContext context,
        [Description("Recipients of the email, separated by ',' or ';'.")] string prompt,
        CancellationToken cancellationToken)
    {
        long index = GetChatIdFromContext(context);
        _logger.LogInformation(nameof(GenerateStableDiffusionImage));
        await _publishEndpoint.Publish(new SDImageCreationRequested(prompt, index), cancellationToken);
    }
}
