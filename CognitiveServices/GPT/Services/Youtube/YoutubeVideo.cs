namespace CognitiveServices.AI.Services.Youtube;

public record YoutubeVideo(List<MemoryStream> AudioStreams, string Title, string Url, string ChannelName, string ClosedCaptions);
