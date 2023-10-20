using MassTransit;

namespace VD.TelegramBot.MessageHandlers
{
    public class FaultMessageHandler : IConsumer<Fault>
    {
        public FaultMessageHandler()
        {
            
        }
        public async Task Consume(ConsumeContext<Fault> context)
        {
            Console.WriteLine(context.Message);
            await Task.CompletedTask;
        }
    }
}
