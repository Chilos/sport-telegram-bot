using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using sport_telegram_bot.Domain;

namespace sport_telegram_bot.Persistence.EntityTypeConfiguration
{
    public class ExerciseRecordConfiguration: IEntityTypeConfiguration<ExerciseRecord>
    {
        public void Configure(EntityTypeBuilder<ExerciseRecord> builder)
        {
            builder.HasKey(record => record.Id);
            builder.HasIndex(record => record.Id).IsUnique();
            builder.Property(record => record.Id).ValueGeneratedOnAdd();
        }
    }
}