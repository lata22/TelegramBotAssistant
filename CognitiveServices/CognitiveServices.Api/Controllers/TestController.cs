using CognitiveServices.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticMemory;

namespace CognitiveServices.Api.Controllers
{
    public class ContentRequest
    {
        public string Content { get; set; }
    }
    [ApiController]
    [Route("test")]
    public class TestController : Controller
    {
        private readonly IChatCompletion _chatCompletion;
        private readonly ISemanticMemoryClient _memoryClient;
        private readonly ApplicationDBContext _dbContext;
        public TestController(
            IChatCompletion chatCompletion,
            ISemanticMemoryClient memoryClient,
            ApplicationDBContext dbContext)
        {
            _chatCompletion = chatCompletion;
            _memoryClient = memoryClient;
            _dbContext = dbContext;
        }
        [HttpGet("summarize/{documentId}")]
        public async Task<ActionResult> Summarize(
            [FromRoute] string documentId,
            CancellationToken cancellationToken,
            [FromQuery] double? temperature = null,
            [FromQuery] double? topP = null,
            [FromQuery] double? presencePenalty = null,
            [FromQuery] double? frequencyPenalty = null)
        {
            var requestSettings = new OpenAIRequestSettings();
            if (temperature != null)
            {
                requestSettings.Temperature = (double)temperature;
            }
            if (topP != null)
            {
                requestSettings.TopP = (double)topP;
            }
            if (presencePenalty != null)
            {
                requestSettings.PresencePenalty = (double)presencePenalty;
            }
            if (frequencyPenalty != null)
            {
                requestSettings.FrequencyPenalty = (double)frequencyPenalty;
            }
            var chatHistory = _chatCompletion.CreateNewChat(
               "Task: The user will supply you with a transcription of a YouTube video. " +
               "Your response must be in the same language as the transcription. " +
               "Your responsibility is to produce a detailed summary, ensuring that the user can utilize it for educational purposes and thorough comprehension. " +
               "Focus on the following:\r\n\r\n" +
               "Detail-Oriented: Extract and present the most significant information.\r\n" +
               "Clarity with Examples: If a piece of information appears abstract or vague, enhance it with concrete examples for clearer understanding.\r\n" +
               "Conciseness: Distill main ideas into concise explanations without sacrificing essential details.\r\n" +
               "Lastly, after the comprehensive summary, please provide a condensed summary of that detailed summary.");
            var youtube = await _dbContext.YoutubeVideos.FirstOrDefaultAsync(y => y.DocumentId == documentId, cancellationToken);
            if (youtube == null)
            {
                return NoContent();
            }
            chatHistory.AddUserMessage(youtube.Transcription);
            var chatCompletionResult = await _chatCompletion.GetChatCompletionsAsync(chatHistory, requestSettings, cancellationToken);
            var result = await chatCompletionResult.First().GetChatMessageAsync();
            return Ok(result.Content);
        }
        [HttpGet]
        public async Task<ActionResult> Test(
            CancellationToken cancellationToken,
            [FromQuery] double? temperature = null,
            [FromQuery] double? topP = null,
            [FromQuery] double? presencePenalty = null,
            [FromQuery] double? frequencyPenalty = null,
            [FromQuery] int? maxTokens = null)
        {
            var requestSettings = new OpenAIRequestSettings();
            if (temperature != null)
            {
                requestSettings.Temperature = (double)temperature;
            }
            if (topP != null)
            {
                requestSettings.TopP = (double)topP;
            }
            if (presencePenalty != null)
            {
                requestSettings.PresencePenalty = (double)presencePenalty;
            }
            if (frequencyPenalty != null)
            {
                requestSettings.FrequencyPenalty = (double)frequencyPenalty;
            }
            if (maxTokens != null)
            {
                requestSettings.MaxTokens = (int)maxTokens;
            }
            var chatHistory = _chatCompletion.CreateNewChat(
                "You have been provided with a user story requirement, a code base and a transcription of a meeting that explains the existing codebase that needs to be changed to meet the new user story. " +
                "Please read both thoroughly. Your task is to generate a comprehensive summary that: " +
                "Clearly outlines the objective(s) of the new user story requirement.\r\n" +
                "Summarizes the key points discussed in the meeting transcription about the existing code that needs modification.\r\n" +
                "Points out any challenges, complications or missing changes foreseen in aligning the existing code with the user story requirements.\r\n" +
                "Provides a succinct but thorough explanation that could serve as a starting point for the development team to understand what needs to be done.\r\n" +
                "Please ensure that your summary captures the essential elements without losing important details.");
            chatHistory.AddUserMessage(System.IO.File.ReadAllText("TextFile1.txt"));
            var chatCompletionResult = await _chatCompletion.GetChatCompletionsAsync(chatHistory, requestSettings, cancellationToken);
            var result = await chatCompletionResult.First().GetChatMessageAsync();
            return Ok(result);
        }

        [Route("/semanticsearch")]
        [HttpGet]
        public async Task<ActionResult> Test(
            CancellationToken cancellationToken,
            [FromQuery] string? youtubeUrl = null,
            [FromQuery] string? author = null,
             [FromQuery] string? title = null)
        {
            // var result = await _memoryClient.SearchAsync("El calentamiento global es una consecuencia del efecto invernadero?", "1145592418", new MemoryFilter()
            // {
            //     { "Url", youtubeUrl},
            //     { "Title", title},
            //     { "Author", author}
            // },
            // -1,
            // cancellationToken);
            // var ask = await _memoryClient.AskAsync("El calentamiento global es una consecuencia del efecto invernadero?", "1145592418", new()
            // {
            //     { "Url", youtubeUrl},
            //     { "Title", title},
            //     { "Author", author}
            // },
            //cancellationToken);
            //await _memoryClient.DeleteDocumentAsync("70861a2d-e8d5-407c-a983-0b33ae8f0192", "1145592418", cancellationToken);
            return Ok();
        }

    }
}
