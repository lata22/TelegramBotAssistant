namespace CognitiveServices.AI.Services.GoogleSearch
{
    public record WebsiteResult(string Title, string URL, string Content)
    {
        public override string ToString()
        {
            return Title + URL + Content;
        }
    }
}
