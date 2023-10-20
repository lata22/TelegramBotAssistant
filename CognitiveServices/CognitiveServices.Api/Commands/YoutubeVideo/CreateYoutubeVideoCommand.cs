using CognitiveServices.Db;
using MediatR;

namespace CognitiveServices.Api.Commands.YoutubeVideo
{
    public record CreateYoutubeVideoCommand(Db.Entities.YoutubeVideo YoutubeVideo) : IRequest<int>;

    public class CreateYoutubeVideoCommandHandler : IRequestHandler<CreateYoutubeVideoCommand, int>
    {
        private readonly ApplicationDBContext _dbContext;

        public CreateYoutubeVideoCommandHandler(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> Handle(CreateYoutubeVideoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.AddAsync(request.YoutubeVideo, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return request.YoutubeVideo.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

}
