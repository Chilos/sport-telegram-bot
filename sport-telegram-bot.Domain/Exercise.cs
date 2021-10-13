namespace sport_telegram_bot.Domain
{
    public sealed class Exercise
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
    }
}