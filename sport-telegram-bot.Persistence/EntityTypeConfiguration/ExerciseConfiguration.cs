using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using sport_telegram_bot.Domain;

namespace sport_telegram_bot.Persistence.EntityTypeConfiguration
{
    public class ExerciseConfiguration: IEntityTypeConfiguration<Exercise>
    {
        public void Configure(EntityTypeBuilder<Exercise> builder)
        {
            builder.HasKey(exercise => exercise.Id);
            builder.HasIndex(exercise => exercise.Id).IsUnique();
            builder.Property(exercise => exercise.Id).ValueGeneratedOnAdd();
        }
    }
}