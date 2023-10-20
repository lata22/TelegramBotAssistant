using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace CognitiveServices.AI.Services.SemanticKernel
{
    public class SemanticKernelFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly SemanticKernelConfig _semanticKernelConfig;
        public SemanticKernelFactory(
            ILoggerFactory loggerFactory,
            SemanticKernelConfig semanticKernelConfig)
        {
            _loggerFactory = loggerFactory;
            _semanticKernelConfig = semanticKernelConfig;
        }

        public IKernel GetKernelWithSmartGPTModel()
        {
            return new KernelBuilder()
                .WithOpenAIChatCompletionService(_semanticKernelConfig.SmartGPTModel, _semanticKernelConfig.OpenAIApiKey, _semanticKernelConfig.OpenAIOrgId)
                .WithOpenAITextEmbeddingGenerationService(_semanticKernelConfig.OpenAIEmbeddingModel, _semanticKernelConfig.OpenAIApiKey, _semanticKernelConfig.OpenAIOrgId)
                .WithQdrantMemoryStore(_semanticKernelConfig.QdrantEndpoint, 1536)
                .WithLoggerFactory(_loggerFactory)
                .Build();
        }
        public IKernel GetKernelWithGeneralGPTModel()
        {
            return new KernelBuilder()
                .WithOpenAIChatCompletionService(_semanticKernelConfig.GeneralGPTModel, _semanticKernelConfig.OpenAIApiKey, _semanticKernelConfig.OpenAIOrgId)
                .WithOpenAITextEmbeddingGenerationService(_semanticKernelConfig.OpenAIEmbeddingModel, _semanticKernelConfig.OpenAIApiKey, _semanticKernelConfig.OpenAIOrgId)
                .WithQdrantMemoryStore(_semanticKernelConfig.QdrantEndpoint, 1536)
                .WithLoggerFactory(_loggerFactory)
                .Build();
        }
    }
}
