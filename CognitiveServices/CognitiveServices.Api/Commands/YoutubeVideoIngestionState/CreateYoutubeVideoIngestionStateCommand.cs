using CognitiveServices.Db;
using MediatR;

namespace CognitiveServices.Api.Commands.YoutubeVideoIngestionState;

public record CreateYoutubeVideoIngestionStateCommand(Db.Entities.YoutubeVideoIngestionState YoutubeVideoIngestionState) : IRequest;

public class CreateYoutubeVideoIngestionStateCommandHandler : IRequestHandler<CreateYoutubeVideoIngestionStateCommand>
{
    private readonly ApplicationDBContext _dbContext;

    public CreateYoutubeVideoIngestionStateCommandHandler(ApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(CreateYoutubeVideoIngestionStateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.AddAsync(request.YoutubeVideoIngestionState, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            throw;
        }
    }
}