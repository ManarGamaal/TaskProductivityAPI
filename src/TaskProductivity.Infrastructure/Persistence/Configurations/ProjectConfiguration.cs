using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskProductivity.Domain.Entities;
using TaskProductivity.Infrastructure.Identity;

namespace TaskProductivity.Infrastructure.Persistence.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.ColorCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.HasIndex(p => p.UserId);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
