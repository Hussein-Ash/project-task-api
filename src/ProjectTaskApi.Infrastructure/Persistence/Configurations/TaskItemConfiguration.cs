using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectTaskApi.Domain.Entities;

namespace ProjectTaskApi.Infrastructure.Persistence.Configurations;

public sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(task => task.Id);

        builder.Property(task => task.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(task => task.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        builder.Property(task => task.Title)
            .HasColumnName("title")
            .HasMaxLength(TaskItem.TitleMaxLength)
            .IsRequired();

        builder.Property(task => task.Completed)
            .HasColumnName("completed")
            .HasDefaultValue(false)
            .IsRequired();

        // Nullable: a task that has never been completed has no completion time.
        builder.Property(task => task.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(task => task.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(task => task.Project)
            .WithMany(project => project.Tasks)
            .HasForeignKey(task => task.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Every task lookup goes through the project, so the FK column carries its own index.
        builder.HasIndex(task => task.ProjectId)
            .HasDatabaseName("ix_tasks_project_id");
    }
}
