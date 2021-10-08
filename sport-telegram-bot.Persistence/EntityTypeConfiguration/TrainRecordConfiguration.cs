using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using sport_telegram_bot.Domain;

namespace sport_telegram_bot.Persistence.EntityTypeConfiguration
{
    public class TrainRecordConfiguration: IEntityTypeConfiguration<TrainRecord>
    {
        public void Configure(EntityTypeBuilder<TrainRecord> builder)
        {
            builder.HasKey(record => record.Id);
            builder.HasIndex(record => record.Id).IsUnique();
            builder.Property(record => record.Id).ValueGeneratedOnAdd();
        }
    }
}