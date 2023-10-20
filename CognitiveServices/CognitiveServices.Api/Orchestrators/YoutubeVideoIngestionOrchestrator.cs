using CognitiveServices.Api.Commands;
using CognitiveServices.Api.Commands.YoutubeVideo;
using CognitiveServices.Api.Commands.YoutubeVideoIngestionState;
using CognitiveServices.Api.Queries.YoutubeVideo;
using CognitiveServices.Api.Queries.YoutubeVideoIngestionState;
using CognitiveServices.Db.Entities;
using Infrastructure.Messaging.Events;
using MassTransit;
using MediatR;

namespace CognitiveServices.Api.Orchestrators;

public class YoutubeVideoIngestionOrchestrator :
    IConsumer<YoutubeVideoDownloadRequested>,
    IConsumer<AudioSegmentationRequested>,
    IConsumer<YoutubeVideoTranscriptionRequested>,
    IConsumer<YoutubeVideoCreated>,
    IConsumer<YoutubeVideoSemanticIngestionRequested>,
    IConsumer<YoutubeVideoTranscriptionUpdated>,
    IConsumer<YoutubeVideoDocumentUpdated>
{
    private readonly ILogger<YoutubeVideoIngestionOrchestrator> _logger;
    private readonly IMediator _mediator;
    public YoutubeVideoIngestionOrchestrator(
        ILogger<YoutubeVideoIngestionOrchestrator> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    //private async Task<YoutubeVideoIngestionState> GetYoutubeIngestionStateAsync(Guid correlationId, CancellationToken cancellationToken)
    //{
    //    return await _dbContext.YoutubeVideoIngestionStates.FindAsync(correlationId, cancellationToken) ??
    //        throw new ArgumentNullException();
    //}

    //private async Task SaveAndPublish<PublishMessage, CurrentMessage>(ConsumeContext<CurrentMessage> context, PublishMessage message)
    //where CurrentMessage : class
    //where PublishMessage : class
    //{
    //    await _dbContext.SaveChangesAsync(context.CancellationToken);
    //    await context.Publish(message, context.CancellationToken);
    //}

    public async Task Consume(ConsumeContext<YoutubeVideoDownloadRequested> context)
    {
        try
        {
            string messageName = nameof(YoutubeVideoDownloadRequested);
            await _mediator.Send(new YoutubeVideoIngestionStateExistsQuery(context.Message.CorrelationId));
            var youtubeResult = await _mediator.Send(new YoutubeDownloadCommand(context.Message.CorrelationId, context.Message.VideoUrl), context.CancellationToken);
            var youtubeIngestionState = new YoutubeVideoIngestionState()
            {
                CorrelationId = context.Message.CorrelationId,
                AudioStreamCacheKey = youtubeResult.CacheKey,
                ChatId = context.Message.TelegramChatId,
                DocumentId = string.Empty,
                Transcription = youtubeResult.YoutubeVideo.ClosedCaptions,
                YoutubeVideoId = null,
                YoutubeVideoUrl = context.Message.VideoUrl,
                SuccessfullySteps = messageName
            };
            await _mediator.Send(new CreateYoutubeVideoIngestionStateCommand(youtubeIngestionState), context.CancellationToken);
            await context.Publish(new YoutubeVideoCreated(
                    context.Message.CorrelationId,
                        context.Message.TelegramChatId,
                        string.Empty,
                        context.Message.VideoUrl,
                        youtubeResult.YoutubeVideo.Title,
                        youtubeResult.YoutubeVideo.ChannelName,
                        youtubeResult.YoutubeVideo.ClosedCaptions), context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    public async Task Consume(ConsumeContext<AudioSegmentationRequested> context)
    {
        try
        {
            var cacheKeys = await _mediator.Send(new AudioSegmentationCommand(context.Message.AudioCacheKey), context.CancellationToken);
            await _mediator.Send(new UpdateYoutubeIngestionStateCommand(
                context.Message.CorrelationId,
                null,
                nameof(AudioSegmentationRequested),
                string.Empty,
                string.Empty),
                context.CancellationToken);
            await context.Publish(new YoutubeVideoTranscriptionRequested(context.Message.CorrelationId, cacheKeys), context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    public async Task Consume(ConsumeContext<YoutubeVideoTranscriptionRequested> context)
    {
        try
        {
            string whisperTranscription = await _mediator.Send(new YoutubeVideoTranscriptionCommand(context.Message.CacheKeys), context.CancellationToken);
            await _mediator.Send(new UpdateYoutubeIngestionStateCommand(
                context.Message.CorrelationId,
                null,
                nameof(YoutubeVideoTranscriptionRequested),
                string.Empty,
                string.Empty),
                context.CancellationToken);
            await context.Publish(new YoutubeVideoTranscriptionUpdated(
                            context.Message.CorrelationId),
                        context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }


    public async Task Consume(ConsumeContext<YoutubeVideoCreated> context)
    {
        try
        {
            var youtubeVideoId = await _mediator.Send(new CreateYoutubeVideoCommand(new YoutubeVideo()
            {
                ChannelName = context.Message.ChannelName,
                CreatedAt = DateTime.UtcNow,
                DocumentId = context.Message.DocumentId,
                TelegramChatId = context.Message.TelegramChatId,
                Title = context.Message.Title,
                Transcription = context.Message.Transcription,
                Url = context.Message.Url
            }));
            var cacheKey = await _mediator.Send(new UpdateYoutubeIngestionStateCommand(
                    context.Message.CorrelationId,
                    youtubeVideoId,
                    nameof(YoutubeVideoCreated),
                    string.Empty,
                    string.Empty),
                context.CancellationToken);
            if (string.IsNullOrWhiteSpace(context.Message.Transcription))
            {
                await context.Publish(new AudioSegmentationRequested(context.Message.CorrelationId, cacheKey));
            }
            else
            {
                await context.Publish(new YoutubeVideoSemanticIngestionRequested(context.Message.CorrelationId, youtubeVideoId));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    public async Task Consume(ConsumeContext<YoutubeVideoSemanticIngestionRequested> context)
    {
        try
        {
            var documentId = await _mediator.Send(new YoutubeVideoSemanticIngestionCommand(context.Message.YoutubeVideoId), context.CancellationToken);
            await _mediator.Send(new UpdateYoutubeIngestionStateCommand(
                        context.Message.CorrelationId,
                        null,
                        nameof(YoutubeVideoSemanticIngestionRequested),
                        documentId,
                        string.Empty),
                        context.CancellationToken);
            var youtubeVideo = await _mediator.Send(
                new GetYoutubeVideoByIdQuery(
                    context.Message.YoutubeVideoId),
                context.CancellationToken);
            await _mediator.Send(new UpdateYoutubeIngestionStateCommand(
                    context.Message.CorrelationId,
                    null,
                    nameof(YoutubeVideoSemanticIngestionRequested),
                    string.Empty,
                    string.Empty),
                  context.CancellationToken);
            await Task.WhenAll(
                context.Publish(new YoutubeVideoDocumentUpdated(context.Message.CorrelationId, context.Message.YoutubeVideoId, documentId), context.CancellationToken),
                context.Publish(new TelegramMessageCreated(youtubeVideo.TelegramChatId, "Video de YouTube recibido!\nTe notificaré cuando termine de procesarlo"), context.CancellationToken),
                context.Publish(new DocumentProcessingStarted(documentId, youtubeVideo.TelegramChatId.ToString(), youtubeVideo.TelegramChatId), context.CancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    public async Task Consume(ConsumeContext<YoutubeVideoTranscriptionUpdated> context)
    {
        try
        {
            var youtubeVideoId = await _mediator.Send(
                new UpdateYoutubeVideoTranscriptionFromStateCommand(
                    context.Message.CorrelationId),
                context.CancellationToken);
            //returns cacheKey?
            await _mediator.Send(new UpdateYoutubeIngestionStateCommand(
                    context.Message.CorrelationId,
                    null,
                    nameof(YoutubeVideoTranscriptionUpdated),
                    string.Empty,
                    string.Empty),
                  context.CancellationToken);
            await context.Publish(new YoutubeVideoSemanticIngestionRequested(
                    context.Message.CorrelationId,
                    youtubeVideoId), context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    public async Task Consume(ConsumeContext<YoutubeVideoDocumentUpdated> context)
    {
        try
        {
            await _mediator.Send(new UpdateYoutubeVideoDocumentIdCommand(
                    context.Message.YoutubeVideoId,
                    context.Message.DocumentId),
                context.CancellationToken);
            await _mediator.Send(new UpdateYoutubeIngestionStateCommand(
                        context.Message.CorrelationId,
                        null,
                        nameof(YoutubeVideoDocumentUpdated),
                        context.Message.DocumentId,
                        string.Empty),
                      context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}