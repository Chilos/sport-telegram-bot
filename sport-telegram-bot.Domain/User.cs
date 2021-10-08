namespace sport_telegram_bot.Domain
{
    public record User
    {
        public int Id { get; set; }
        public string TelegramId { get; set; }
        public string Name { get; set; }
    }
}