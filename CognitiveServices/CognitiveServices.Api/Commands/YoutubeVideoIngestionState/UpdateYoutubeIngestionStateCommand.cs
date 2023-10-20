using CognitiveServices.Db;
using CognitiveServices.Db.Exceptions;
using MediatR;

namespace CognitiveServices.Api.Commands.YoutubeVideoIngestionState
{
    public record UpdateYoutubeIngestionStateCommand(Guid CorrelationId, int? YoutubeVideoId, string SuccessfullStep, string DocumentId, string Transcription) : IRequest<string>;

    public class UpdateYoutubeIngestionStateCommandHandler : IRequestHandler<UpdateYoutubeIngestionStateCommand, string>
    {
        private readonly ApplicationDBContext _dbContext;

        public UpdateYoutubeIngestionStateCommandHandler(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> Handle(UpdateYoutubeIngestionStateCommand request, CancellationToken cancellationToken)
        {
            var youtubeState = await _dbContext.YoutubeVideoIngestionStates.FindAsync(request.CorrelationId, cancellationToken);
            if (youtubeState == null)
            {
                throw new EntityNotFoundException($"Entity {nameof(YoutubeVideoIngestionState)} with CorrelationId {request.CorrelationId} was not found");
            }
            if (request.YoutubeVideoId != null)
            {
                youtubeState.YoutubeVideoId = request.YoutubeVideoId;
            }
            if (!string.IsNullOrWhiteSpace(request.SuccessfullStep))
            {
                youtubeState.SuccessfullySteps += "," + request.SuccessfullStep;
            }
            if (!string.IsNullOrWhiteSpace(request.DocumentId))
            {
                youtubeState.DocumentId = request.DocumentId;
            }
            if (!string.IsNullOrWhiteSpace(request.Transcription))
            {
                youtubeState.Transcription = request.Transcription;
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            return youtubeState.AudioStreamCacheKey;
        }
    }
}
