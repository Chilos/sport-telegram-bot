namespace sport_telegram_bot.Domain
{
    public class ExerciseRecord
    {
        public int Id { get; set; }
        public Exercise Exercise { get; set; }
        public int Repetitions { get; set; }
        public int Weight { get; set; }
    }
}