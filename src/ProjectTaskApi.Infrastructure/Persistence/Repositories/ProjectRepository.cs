using Microsoft.EntityFrameworkCore;
using ProjectTaskApi.Application.Projects.Interfaces;
using ProjectTaskApi.Domain.Entities;

namespace ProjectTaskApi.Infrastructure.Persistence.Repositories;

public sealed class ProjectRepository(AppDbContext dbContext) : IProjectRepository
{
    public async Task<(IReadOnlyList<Project> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Projects.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(project => project.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Project?> GetByIdWithTasksAsync(
        Guid id,
        bool? completed,
        CancellationToken cancellationToken) =>
        dbContext.Projects
            .AsNoTracking()
            .Include(project => project.Tasks
                .Where(task => completed == null || task.Completed == completed))
            .SingleOrDefaultAsync(project => project.Id == id, cancellationToken);

    /// <summary>
    /// Tracked deliberately: callers fetch a project in order to mutate it through the
    /// domain's <c>Update</c> method, and EF needs to observe those changes.
    /// </summary>
    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Projects.SingleOrDefaultAsync(project => project.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Projects.AnyAsync(project => project.Id == id, cancellationToken);

    public async Task AddAsync(Project project, CancellationToken cancellationToken)
    {
        await dbContext.Projects.AddAsync(project, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Project project, CancellationToken cancellationToken)
    {
        dbContext.Projects.Update(project);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
