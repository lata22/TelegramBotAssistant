using CognitiveServices.Db;
using CognitiveServices.Db.Exceptions;
using Infrastructure.Messaging.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CognitiveServices.Api.Queries.YoutubeVideoIngestionState;

public record YoutubeVideoIngestionStateExistsQuery(Guid CorrelationId) : IRequest;

public class YoutubeVideoIngestionStateExistsQueryHandler : IRequestHandler<YoutubeVideoIngestionStateExistsQuery>
{
    private readonly ApplicationDBContext _dbContext;

    public YoutubeVideoIngestionStateExistsQueryHandler(ApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(YoutubeVideoIngestionStateExistsQuery request, CancellationToken cancellationToken)
    {
        bool exists = await _dbContext.YoutubeVideoIngestionStates
           .AnyAsync(x => x.CorrelationId == request.CorrelationId &&
           x.SuccessfullySteps.Length > nameof(YoutubeVideoDownloadRequested).Length, cancellationToken);

        if (exists)
        {
            throw new DuplicateKeyException($"Message {nameof(YoutubeVideoDownloadRequested)} contains already existing state with CorrelationId {request.CorrelationId} running");
        }
    }
}

