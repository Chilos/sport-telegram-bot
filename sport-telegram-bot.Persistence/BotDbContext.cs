using Microsoft.EntityFrameworkCore;
using sport_telegram_bot.Application.Abstract;
using sport_telegram_bot.Domain;
using sport_telegram_bot.Persistence.EntityTypeConfiguration;

namespace sport_telegram_bot.Persistence
{
    public class BotDbContext: DbContext, IBotDbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<ExerciseRecord> ExerciseRecord { get; set; }
        public DbSet<TrainRecord> TrainRecord { get; set; }

        public BotDbContext(DbContextOptions<BotDbContext> options): base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ExerciseConfiguration());
            modelBuilder.ApplyConfiguration(new ExerciseRecordConfiguration());
            modelBuilder.ApplyConfiguration(new TrainRecordConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}