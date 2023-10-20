using CognitiveServices.Db;
using CognitiveServices.Db.Exceptions;
using MediatR;
using Microsoft.SemanticMemory;

namespace CognitiveServices.Api.Commands.YoutubeVideo
{
    public record YoutubeVideoSemanticIngestionCommand(int YoutubeVideoId) : IRequest<string>;

    public class YoutubeVideoSemanticIngestionCommandHandler : IRequestHandler<YoutubeVideoSemanticIngestionCommand, string>
    {
        private readonly ISemanticMemoryClient _semanticMemoryClient;
        private readonly ApplicationDBContext _dbContext;

        public YoutubeVideoSemanticIngestionCommandHandler(
            ISemanticMemoryClient semanticMemoryClient,
            ApplicationDBContext dbContext)
        {
            _semanticMemoryClient = semanticMemoryClient;
            _dbContext = dbContext;
        }

        //returns documentId
        public async Task<string> Handle(YoutubeVideoSemanticIngestionCommand request, CancellationToken cancellationToken)
        {
            var youtubeVideo = await _dbContext.YoutubeVideos
                .FindAsync(request.YoutubeVideoId, cancellationToken) ??
                throw new EntityNotFoundException($"Entity {nameof(YoutubeVideo)} with Id {request.YoutubeVideoId} was not found");

            var tagCollection = new TagCollection()
            {
                {nameof(youtubeVideo.Title) , youtubeVideo.Title},
                {nameof(youtubeVideo.ChannelName) , youtubeVideo.ChannelName},
                {nameof(youtubeVideo.Url) , youtubeVideo.Url},
                {nameof(youtubeVideo.CreatedAt) , youtubeVideo.CreatedAt.ToString()}
            };
            return await _semanticMemoryClient.ImportTextAsync(
                youtubeVideo.Transcription,
                Guid.NewGuid().ToString(),
                tagCollection,
                youtubeVideo.TelegramChatId.ToString(),
                null,
                cancellationToken);
        }
    }
}
