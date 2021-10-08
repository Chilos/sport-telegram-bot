namespace sport_telegram_bot.Persistence
{
    public class DbInitializer
    {
        public static void Initialize(BotDbContext context)
        {
            context.Database.EnsureCreated();
        }
    }
}