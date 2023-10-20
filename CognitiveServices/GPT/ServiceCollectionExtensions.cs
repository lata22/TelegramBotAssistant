using CognitiveServices.AI.Services.AudioSplitters;
using CognitiveServices.AI.Services.Email;
using CognitiveServices.AI.Services.GoogleSearch;
using CognitiveServices.AI.Services.GPT;
using CognitiveServices.AI.Services.SemanticKernel;
using CognitiveServices.AI.Services.SemanticKernel.Plugins;
using CognitiveServices.AI.Services.VideoAndAudioConverters;
using CognitiveServices.AI.Services.Whisper;
using CognitiveServices.AI.Services.Youtube;
using Google.Apis.CustomSearchAPI.v1;
using Google.Apis.Services;
using Infrastructure.Firebase.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;
using Microsoft.SemanticKernel.Connectors.Memory.Qdrant;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticMemory;
using OpenAI.Extensions;

namespace CognitiveServices.AI
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddAIServices(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddOpenAiServices(configuration)
                .AddSemanticMemory()
                .AddSemanticKernel(configuration)
                .AddEmailService(configuration)
                .AddSemanticKernellSkills();


            services.AddSingleton<IGPTCompletionService, GPTCompletionService>();
            services.AddSingleton<ISequentialPlannerFactory, SequentialPlannerFactory>();
            services.AddSingleton<IMemoryStore, QdrantMemoryStore>(sp =>
            {
                var loggerFactory = sp.GetService<ILoggerFactory>();
                var httpHandlerFactory = sp.GetService<IDelegatingHandlerFactory>();
                var semanticKernelConfig = sp.GetService<SemanticKernelConfig>();
                var client = new QdrantVectorDbClient(semanticKernelConfig!.QdrantEndpoint, 1536, loggerFactory);
                return new QdrantMemoryStore(client, loggerFactory);
            });
            services.AddSingleton<ITextEmbeddingGeneration, OpenAITextEmbeddingGeneration>(sp =>
            {
                var config = sp.GetService<SemanticKernelConfig>();
                return new OpenAITextEmbeddingGeneration(config!.OpenAIEmbeddingModel, config.OpenAIApiKey, config.OpenAIOrgId);
            });
            services.AddSingleton<ISemanticTextMemory, SemanticTextMemory>();
            services.AddSingleton(sp =>
            {
                var googleConfig = sp.GetService<GoogleConfig>() ??
                    throw new ArgumentNullException("GoogleConfig section cannot be null or empty in the appsettings.json");

                return new CustomSearchAPIService(new BaseClientService.Initializer()
                {
                    ApiKey = googleConfig!.SearchApiKey,
                });
            });
            services.AddSingleton<IGoogleSearchService, GoogleSearchService>();
            services.AddSingleton<IContentSummarizer, ContentSummarizer>();
            return services;
        }

        private static IServiceCollection AddEmailService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IGmailService, AuthenticatedGmailService>();
            services.AddSingleton<IHotmailService, HotmailService>();
            return services;
        }

        private static IServiceCollection AddOpenAiServices(this IServiceCollection services, IConfiguration configuration)
        {
            var FfmpegBinaryFolderPath = configuration.GetValue<string>("FfmpegBinaryFolderPath") ??
                throw new ArgumentNullException("FfmpegBinaryFolderPath cannot be null or empty in the appsettings.json");

            var openaiApiKey = configuration.GetValue<string>("OpenAIApiKey") ??
                throw new ArgumentNullException("OpenAIApiKey cannot be null or empty in the appsettings.json");

            services.AddOpenAIService(options =>
            {
                options.ApiKey = openaiApiKey;
            })
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(5); // Adjust as needed
            });
            services.AddSingleton<IWhisperService, WhisperService>();
            services.AddSingleton<IAudioFormatConverter, AudioFormatConverter>(a => new AudioFormatConverter(FfmpegBinaryFolderPath));
            services.AddSingleton<IYoutubeVideoDownloader, YoutubeVideoDownloader>();
            services.AddSingleton<IMP3AudioSplitter, MP3AudioSplitter>();
            services.AddSingleton<IAudioStreamDownloader, AudioStreamDownloader>();
            services.AddSingleton<IMP4AudioSplitter, MP4AudioSplitter>(a => new MP4AudioSplitter(FfmpegBinaryFolderPath));
            services.AddSingleton<IVideoToAudioConverter, VideoToAudioConverter>(_ => new VideoToAudioConverter(FfmpegBinaryFolderPath));
            services.AddSingleton<IYoutubeClosedCaptionDownloader, YoutubeClosedCaptionDownloader>();
            return services;
        }

        private static IServiceCollection AddSemanticKernel(this IServiceCollection services, IConfiguration config)
        {
            string key = config.GetSection("SemanticMemory").GetSection("Services").GetSection("OpenAI").GetValue<string>("APIKey") ??
                 throw new ArgumentNullException("SemanticMemory.Services.OpenAI.APIKey cannot be null or empty in the appsettings.json");

            string orgId = config.GetSection("SemanticMemory").GetSection("Services").GetSection("OpenAI").GetValue<string>("OrgId") ??
                 throw new ArgumentNullException("SemanticMemory.Services.OpenAI.OrgId cannot be null or empty in the appsettings.json");

            string embeddingsModel = config.GetSection("SemanticMemory").GetSection("Services").GetSection("OpenAI").GetValue<string>("EmbeddingModel") ??
                 throw new ArgumentNullException("SemanticMemory.Services.OpenAI.EmbeddingModel cannot be null or empty in the appsettings.json");

            string qdrantEndpoint = config.GetSection("SemanticMemory").GetSection("Services").GetSection("Qdrant").GetValue<string>("Endpoint") ??
                 throw new ArgumentNullException("SemanticMemory.Services.Qdrant.Endpoint cannot be null or empty in the appsettings.json");

            var semanticKernelConfig = new SemanticKernelConfig()
            {
                GeneralGPTModel = "gpt-3.5-turbo-16k",
                SmartGPTModel = "gpt-4",
                OpenAIApiKey = key,
                OpenAIEmbeddingModel = embeddingsModel,
                OpenAIOrgId = orgId,
                QdrantEndpoint = qdrantEndpoint,
            };

            services.AddSingleton(semanticKernelConfig);
            services.AddSingleton<IChatCompletion, OpenAIChatCompletion>(_ => new OpenAIChatCompletion(semanticKernelConfig.GeneralGPTModel, semanticKernelConfig.OpenAIApiKey));
            services.AddSingleton<SemanticKernelFactory>();
            services.AddScoped(sp =>
            {
               //new Microsoft.SemanticKernel.Plugins.Memory.MemoryBuilder().WithQdrantMemoryStore
                var logger = sp.GetRequiredService<ILoggerFactory>();
                IKernel semanticKernel = new KernelBuilder()
                .WithOpenAIChatCompletionService("gpt-3.5-turbo-16k", key, orgId)
                .WithOpenAITextEmbeddingGenerationService(embeddingsModel, key, orgId)
                .WithQdrantMemoryStore(qdrantEndpoint, 1536)
                .WithLoggerFactory(logger)
                .Build();
                return semanticKernel;
            });
            return services;
        }

        private static IServiceCollection AddSemanticKernellSkills(this IServiceCollection services)
        {
            services.AddSingleton<BasePlugin, EmailPlugin>();
            services.AddSingleton<BasePlugin, ChatPlugin>();
            services.AddSingleton<BasePlugin, TelegramPlugin>();
            services.AddSingleton<BasePlugin, QdrantMemoryPlugin>();
            services.AddSingleton<BasePlugin, TranslatePlugin>();
            services.AddSingleton<BasePlugin, GoogleWebSearchPlugin>();
            services.AddSingleton<BasePlugin, StableDiffusionSkill>();
            services.AddSingleton<TextMemoryPlugin>();
            services.AddSingleton<TimePlugin>();
            return services;
        }

        private static IServiceCollection AddSemanticMemory(this IServiceCollection services)
        {
            ISemanticMemoryClient memory = new MemoryClientBuilder(services)
                .FromAppSettings()
                .Build();
            services.AddSingleton(memory);
            return services;
        }
    }
}
