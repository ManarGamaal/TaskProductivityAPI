using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskProductivity.Domain.Entities;
using TaskProductivity.Infrastructure.Identity;

namespace TaskProductivity.Infrastructure.Persistence.Configurations;

public sealed class TimeLogConfiguration : IEntityTypeConfiguration<TimeLog>
{
    public void Configure(EntityTypeBuilder<TimeLog> builder)
    {
        builder.ToTable("TimeLogs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.StartTimeUtc)
            .IsRequired();

        builder.Property(l => l.DurationInMinutes)
            .IsRequired();

        builder.Property(l => l.Notes)
            .HasMaxLength(500);

        builder.Property(l => l.UserId)
            .IsRequired();

        builder.Ignore(l => l.IsRunning);

        builder.HasIndex(l => new { l.UserId, l.EndTimeUtc });
        builder.HasIndex(l => new { l.UserId, l.StartTimeUtc });

        builder.HasOne(l => l.TaskItem)
            .WithMany(t => t.TimeLogs)
            .HasForeignKey(l => l.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
