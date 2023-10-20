using CognitiveServices.Db;
using CognitiveServices.Db.Exceptions;
using MediatR;

namespace CognitiveServices.Api.Queries.YoutubeVideo;

public record GetYoutubeVideoByIdQuery(int YoutubeVideoId) : IRequest<Db.Entities.YoutubeVideo>;

public class GetYoutubeVideoByIdQueryHandler : IRequestHandler<GetYoutubeVideoByIdQuery, Db.Entities.YoutubeVideo>
{
    private readonly ApplicationDBContext _dbContext;

    public GetYoutubeVideoByIdQueryHandler(ApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Db.Entities.YoutubeVideo> Handle(GetYoutubeVideoByIdQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.YoutubeVideos.FindAsync(request.YoutubeVideoId, cancellationToken) ??
             throw new EntityNotFoundException($"Entity {nameof(YoutubeVideo)} with Id {request.YoutubeVideoId} was not found");
    }
}


