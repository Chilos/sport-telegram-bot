namespace sport_telegram_bot.Domain
{
    public sealed class User
    {
        public int? Id { get; set; }
        public long TelegramId { get; set; }
        public string Name { get; set; }
    }
}