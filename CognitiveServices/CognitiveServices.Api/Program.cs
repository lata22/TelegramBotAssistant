using CognitiveServices.AI;
using CognitiveServices.Db.Extensions;
using Infrastructure.Firebase.Extensions;
using Infrastructure.Messaging.Events;
using Infrastructure.Messaging.Extensions;
using Infrastructure.Redis.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configure Logging
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();
//builder.Logging.AddDebug();
//builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add services to the container.

var currentAssembly = Assembly.GetExecutingAssembly();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services
    .AddGoogleServices(builder.Configuration)
    .AddRedisServices(builder.Configuration)
    .AddDbContextServices(builder.Configuration)
    .AddMassTransitRabbitMQ(
        configuration: builder.Configuration,
        eventsToProduce: new Type[]
        {
            typeof(GptResponseProcessed),
            typeof(YoutubeTranscriptionCompleted),
            typeof(YoutubeVideoDownloadRequested),
            typeof(YoutubeVideoCreated),
            typeof(AudioSegmentationRequested),
            typeof(AudioTranscriptionDocumentUpdated),
            typeof(AudioTranscriptionCreated),
            typeof(AudioTranscriptionIngestionRequested),
            typeof(YoutubeVideoTranscriptionRequested),
            typeof(YoutubeVideoTranscriptionUpdated),
            typeof(YoutubeVideoSemanticIngestionRequested),
            typeof(YoutubeVideoDocumentUpdated),
        },
        messageHandlersAssembly: currentAssembly)
    .AddMediatR((config) =>
     {
         config.RegisterServicesFromAssembly(currentAssembly);
     })
    .AddAIServices(builder.Configuration);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
