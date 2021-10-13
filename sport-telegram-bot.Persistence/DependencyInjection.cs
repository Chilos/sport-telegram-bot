using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using sport_telegram_bot.Application.Abstract;

namespace sport_telegram_bot.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration["DATABASE_URL"];
            services.AddDbContext<BotDbContext>(options =>
            {
                options.UseNpgsql(PostgresConnectionStringFactory.GetConnectionStringFromUrl(connectionString));
            }, ServiceLifetime.Singleton);
            services.AddSingleton<IBotDbContext>(provider => provider.GetService<BotDbContext>());
            return services;
        }
    }
}