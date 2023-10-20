using MassTransit;
using System.Reflection;
using Telegram.Bot;
using TelegramBot;
using TelegramBot.Interfaces;

namespace VD.TelegramBot.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTelegramBotServices(this IServiceCollection services, IConfiguration config, Assembly telegramImplementationsAssembly)
        {
            //Telegram bot services
            string token = config.GetValue<string>("TelegramBotToken") ??
                 throw new ArgumentNullException($"{nameof(token)} cannot be null, add to the appsettings a property called TelegramBotToken with the token value");

            services.AddSingleton<ITelegramBotClient, TelegramBotClient>(_ => new TelegramBotClient(token));
            services.AddSingletonServices(new Type[]
            {
                typeof(ITelegramBotSingleton),
            }, telegramImplementationsAssembly)
            .AddHostedService<TelegramBotHostedService>();
            return services;
        }
        public static IServiceCollection RegisterAllTypes<T>(this IServiceCollection services, Assembly assembly)
        {
            var typesFromAssemblies = assembly.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T)));
            foreach (var type in typesFromAssemblies)
                services.Add(new ServiceDescriptor(typeof(T), type, ServiceLifetime.Singleton));
            return services;
        }
        private static IServiceCollection AddServices(this IServiceCollection services, Type[] types, ServiceLifetime serviceLifeTime, Assembly telegramImplementationsAssembly)
        {
            for (int i = 0; i < types.Length; i++)
            {
                var originalAssembly = Assembly.GetAssembly(types[i]);
                var typesInfoExecutionAssembly = telegramImplementationsAssembly
                    .DefinedTypes
                    .Where(t => t.IsClass &&
                                !t.IsAbstract &&
                                t.GetInterfaces()
                                .Any(type => type.Name == types[i].Name))
                    .ToList();

                if (originalAssembly!.GetName().Name != telegramImplementationsAssembly.GetName().Name)
                {
                    typesInfoExecutionAssembly.AddRange(
                        originalAssembly
                        .DefinedTypes
                        .Where(t => t.IsClass &&
                                    !t.IsAbstract &&
                                    t.ImplementedInterfaces
                                    .Any(type => type.Name == types[i].Name))
                        .ToList());
                }
                foreach (var typeInfo in typesInfoExecutionAssembly)
                {
                    foreach (var implementedInterface in typeInfo.ImplementedInterfaces)
                    {
                        switch (serviceLifeTime)
                        {
                            case ServiceLifetime.Scoped:
                                services.AddScoped(implementedInterface, typeInfo);
                                break;
                            case ServiceLifetime.Singleton:
                                services.AddSingleton(implementedInterface, typeInfo);
                                break;
                            case ServiceLifetime.Transient:
                                services.AddTransient(implementedInterface, typeInfo);
                                break;
                        }
                    }
                }
            }
            return services;
        }

        public static IServiceCollection AddSingletonServices(this IServiceCollection services, Type[] types, Assembly telegramImplementationsAssembly)
        {
            return AddServices(services, types, ServiceLifetime.Singleton, telegramImplementationsAssembly);
        }
    }
}
