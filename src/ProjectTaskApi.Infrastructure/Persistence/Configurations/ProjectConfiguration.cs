using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectTaskApi.Domain.Entities;

namespace ProjectTaskApi.Infrastructure.Persistence.Configurations;

/// <summary>
/// Names are mapped to snake_case explicitly rather than through a naming-convention
/// package: one extra line per property, and one fewer dependency.
/// </summary>
public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(project => project.Id);

        builder.Property(project => project.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(project => project.Name)
            .HasColumnName("name")
            .HasMaxLength(Project.NameMaxLength)
            .IsRequired();

        builder.Property(project => project.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // The domain exposes tasks as a read-only view, so EF populates the backing field directly.
        builder.Metadata
            .FindNavigation(nameof(Project.Tasks))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
