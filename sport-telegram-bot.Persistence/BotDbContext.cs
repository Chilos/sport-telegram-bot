using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using sport_telegram_bot.Application.Abstract;
using sport_telegram_bot.Domain;
using sport_telegram_bot.Persistence.EntityTypeConfiguration;

namespace sport_telegram_bot.Persistence
{
    public sealed class BotDbContext: DbContext, IBotDbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<ExerciseRecord> ExerciseRecord { get; set; }
        public DbSet<TrainRecord> TrainRecord { get; set; }

        public BotDbContext()
        {
        }
        
        public BotDbContext(DbContextOptions<BotDbContext> options): base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
        { 
            if (!optionsBuilder.IsConfigured) 
            { 
                optionsBuilder.UseNpgsql("Host=ec2-34-254-120-2.eu-west-1.compute.amazonaws.com; Port=5432; User Id=cewgrqazkykkcv; Password=5866b4dc56122e49dfbd67629a75ce13236a222c6075f67e7d7cb879f87bdb73; Database=d94gkr5s9f2mc7; SSL Mode=Require;Trust Server Certificate=true"); 
            } 
        }
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