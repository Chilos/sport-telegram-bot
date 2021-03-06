using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using sport_telegram_bot.Application;
using sport_telegram_bot.Persistence;
using Telegram.Bot;

namespace sport_telegram_bot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                try
                {
                    var context = serviceProvider.GetService<BotDbContext>()!;
                    context.Database.Migrate();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddApplication();
                    services.AddPersistence(hostContext.Configuration);
                    services.AddSingleton(new TelegramBotClient("2033376592:AAGRSZZP_b-wJaE3HV0UY6bexnrnNW66Euo"));
                    services.AddHostedService<Bot>();
                });
    }
}