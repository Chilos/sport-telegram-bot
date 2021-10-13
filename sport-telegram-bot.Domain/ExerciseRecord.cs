namespace sport_telegram_bot.Domain
{
    public sealed class ExerciseRecord
    {
        public int Id { get; set; }
        public TrainRecord TrainRecord { get; set; }
        public Exercise Exercise { get; set; }
        public int Repetitions { get; set; }
        public int Weight { get; set; }
    }
}