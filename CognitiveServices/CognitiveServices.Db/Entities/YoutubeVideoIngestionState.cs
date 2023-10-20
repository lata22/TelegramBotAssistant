using System.ComponentModel.DataAnnotations;

namespace CognitiveServices.Db.Entities;

public class YoutubeVideoIngestionState
{
    [Key]
    public Guid CorrelationId { get; set; }
    public string SuccessfullySteps { get; set; }
    public string AudioStreamCacheKey { get; set; } = string.Empty;
    public long ChatId { get; set; }
    public string YoutubeVideoUrl { get; set; } = string.Empty;
    public int? YoutubeVideoId { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public string Transcription { get; set; } = string.Empty;

}

