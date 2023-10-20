using Infrastructure.Firebase.Extensions;
using Infrastructure.Messaging.Events;
using Infrastructure.Messaging.Extensions;
using Infrastructure.Redis.Extensions;
using System.Reflection;
using VD.TelegramBot.Db.Extensions;
using VD.TelegramBot.Extensions;

var builder = WebApplication.CreateBuilder(args);

//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();
//builder.Logging.AddDebug();
//builder.Logging.SetMinimumLevel(LogLevel.Trace);

builder.Services.AddControllers();

var currentAssembly = Assembly.GetExecutingAssembly();

builder.Services
.AddGoogleServices(builder.Configuration)
.AddRedisServices(builder.Configuration)
.AddDbContextServices(builder.Configuration)
.AddMassTransitRabbitMQ(
    configuration: builder.Configuration,
    eventsToProduce: new[]
    {
        typeof(FaceDetectionRequested),
        typeof(FaceDetectionVideoRequested),
        typeof(AudioTranscriptionRequested),
        typeof(ChatSessionCreated),
        typeof(DocumentCreated),
        typeof(DocumentSemanticIngestionRequested),
        typeof(GptResponseRequested),
        typeof(YoutubeVideoDownloadRequested),
    },
   messageHandlersAssembly: currentAssembly)
.AddMediatR((config) =>
{
    config.RegisterServicesFromAssembly(currentAssembly);
})
.AddTelegramBotServices(builder.Configuration, currentAssembly);

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
