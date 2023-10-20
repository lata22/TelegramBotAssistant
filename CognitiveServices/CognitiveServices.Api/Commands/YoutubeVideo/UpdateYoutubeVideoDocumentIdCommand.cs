using CognitiveServices.Db;
using CognitiveServices.Db.Exceptions;
using MediatR;

namespace CognitiveServices.Api.Commands.YoutubeVideo
{
    public record UpdateYoutubeVideoDocumentIdCommand(int YoutubeVideoId, string DocumentId) : IRequest;

    public class UpdateYoutubeVideoDocumentIdCommandHandler : IRequestHandler<UpdateYoutubeVideoDocumentIdCommand>
    {
        private readonly ApplicationDBContext _dbContext;

        public UpdateYoutubeVideoDocumentIdCommandHandler(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(UpdateYoutubeVideoDocumentIdCommand request, CancellationToken cancellationToken)
        {
            var youtubeVideo = await _dbContext.YoutubeVideos
                 .FindAsync(request.YoutubeVideoId, cancellationToken) ??
                 throw new EntityNotFoundException($"Entity {nameof(YoutubeVideo)} with Id {request.YoutubeVideoId} was not found");
            youtubeVideo.DocumentId = request.DocumentId;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
