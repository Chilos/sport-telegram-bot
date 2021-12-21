using System;
using System.Collections.Generic;

namespace sport_telegram_bot.Domain
{
    public sealed class TrainRecord
    {
        public int Id { get; set; }
        public User User { get; set; }
        public DateOnly DateAt { get; set; }
        public List<ExerciseRecord> Exercises { get; set; }
    }
}