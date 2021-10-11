using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using sport_telegram_bot.Domain;

namespace sport_telegram_bot.Application.Abstract
{
    public interface IBotDbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<ExerciseRecord> ExerciseRecord { get; set; }
        public DbSet<TrainRecord> TrainRecord { get; set; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}