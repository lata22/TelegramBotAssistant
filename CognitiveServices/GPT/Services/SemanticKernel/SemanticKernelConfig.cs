namespace CognitiveServices.AI.Services.SemanticKernel
{
    public class SemanticKernelConfig
    {
        public string OpenAIApiKey { get; set; }
        public string OpenAIOrgId { get; set; }
        public string OpenAIEmbeddingModel { get; set; }
        public string QdrantEndpoint { get; set; }
        public string GeneralGPTModel { get; set; }
        public string SmartGPTModel { get; set; }
    }
}
