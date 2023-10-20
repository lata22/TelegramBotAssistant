using CognitiveServices.Db;
using CognitiveServices.Db.Exceptions;
using MediatR;

namespace CognitiveServices.Api.Commands.YoutubeVideo
{
    public record UpdateYoutubeVideoTranscriptionFromStateCommand(Guid CorrelationId) : IRequest<int>;
    public class UpdateYoutubeVideoTranscriptionFromStateCommandHandler : IRequestHandler<UpdateYoutubeVideoTranscriptionFromStateCommand, int>
    {
        private readonly ApplicationDBContext _dbContext;

        public UpdateYoutubeVideoTranscriptionFromStateCommandHandler(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> Handle(UpdateYoutubeVideoTranscriptionFromStateCommand request, CancellationToken cancellationToken)
        {
            var state = await _dbContext.YoutubeVideoIngestionStates.FindAsync(request.CorrelationId, cancellationToken) ??
                throw new EntityNotFoundException($"Entity {nameof(YoutubeVideoIngestionState)} with CorrelationId {request.CorrelationId} was not found");
            var youtubeVideo = await _dbContext.YoutubeVideos.FindAsync(state.YoutubeVideoId, cancellationToken) ??
                throw new EntityNotFoundException($"Entity {nameof(YoutubeVideo)} with Id {state.YoutubeVideoId} was not found");
            youtubeVideo.Transcription = state.Transcription;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return youtubeVideo.Id;
        }
    }
}
