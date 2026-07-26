using Moq;
using ProjectTaskApi.Application.Common;
using ProjectTaskApi.Application.Projects.Dtos;
using ProjectTaskApi.Application.Projects.Interfaces;
using ProjectTaskApi.Application.Projects.Services;
using ProjectTaskApi.Domain.Entities;
using ProjectTaskApi.Domain.Exceptions;
using Shouldly;

namespace ProjectTaskApi.UnitTests.Projects;

public sealed class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _projectRepository = new(MockBehavior.Strict);
    private readonly ProjectService _sut;

    public ProjectServiceTests() => _sut = new ProjectService(_projectRepository.Object);

    [Fact]
    public async Task GetPagedAsync_WithProjects_ReturnsCorrectPaginationMetadata()
    {
        var projects = new List<Project> { Project.Create("First"), Project.Create("Second") };
        _projectRepository
            .Setup(repository => repository.GetPagedAsync(2, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((projects, 5));

        var result = await _sut.GetPagedAsync(
            new PageRequest { Page = 2, PageSize = 2 },
            CancellationToken.None);

        result.Page.ShouldBe(2);
        result.PageSize.ShouldBe(2);
        result.TotalCount.ShouldBe(5);
        result.TotalPages.ShouldBe(3);
        result.Items.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetPagedAsync_WhenNoProjects_ReturnsEmptyResult()
    {
        _projectRepository
            .Setup(repository => repository.GetPagedAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(([], 0));

        var result = await _sut.GetPagedAsync(new PageRequest(), CancellationToken.None);

        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
        result.TotalPages.ShouldBe(0);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProjectExists_ReturnsProjectWithTasks()
    {
        var projectId = Guid.CreateVersion7();
        var project = TestEntities.ProjectWith(
            "Website Redesign",
            TaskItem.Create(projectId, "Design homepage"),
            TaskItem.Create(projectId, "Write copy"));

        _projectRepository
            .Setup(repository => repository.GetByIdWithTasksAsync(
                project.Id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var result = await _sut.GetByIdAsync(project.Id, null, CancellationToken.None);

        result.Id.ShouldBe(project.Id);
        result.Name.ShouldBe("Website Redesign");
        result.Tasks.Count.ShouldBe(2);
        result.Tasks.Select(task => task.Title)
            .ShouldBe(["Design homepage", "Write copy"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProjectDoesNotExist_ThrowsProjectNotFoundException()
    {
        var missingId = Guid.CreateVersion7();
        _projectRepository
            .Setup(repository => repository.GetByIdWithTasksAsync(
                missingId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var exception = await Should.ThrowAsync<ProjectNotFoundException>(
            () => _sut.GetByIdAsync(missingId, null, CancellationToken.None));

        exception.Message.ShouldContain(missingId.ToString());
    }

    [Fact]
    public async Task GetByIdAsync_WithCompletedFilter_ReturnsOnlyMatchingTasks()
    {
        var projectId = Guid.CreateVersion7();
        // The repository applies the filter in SQL, so it returns an already-filtered graph.
        var project = TestEntities.ProjectWith(
            "Website Redesign",
            TestEntities.CompletedTask(projectId, "Ship it"));

        _projectRepository
            .Setup(repository => repository.GetByIdWithTasksAsync(
                project.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var result = await _sut.GetByIdAsync(project.Id, completed: true, CancellationToken.None);

        result.Tasks.ShouldHaveSingleItem().Completed.ShouldBeTrue();
        // The filter must reach the repository; filtering in memory would defeat the point.
        _projectRepository.Verify(
            repository => repository.GetByIdWithTasksAsync(
                project.Id, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithValidName_PersistsProject()
    {
        Project? persisted = null;
        _projectRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .Callback<Project, CancellationToken>((project, _) => persisted = project)
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync(
            new CreateProjectRequest { Name = "Website Redesign" },
            CancellationToken.None);

        persisted.ShouldNotBeNull();
        persisted.Name.ShouldBe("Website Redesign");
        result.Id.ShouldBe(persisted.Id);
        result.Name.ShouldBe("Website Redesign");
    }

    [Fact]
    public async Task CreateAsync_WithUntrimmedName_StoresTrimmedName()
    {
        Project? persisted = null;
        _projectRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .Callback<Project, CancellationToken>((project, _) => persisted = project)
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync(
            new CreateProjectRequest { Name = "   Website Redesign   " },
            CancellationToken.None);

        persisted!.Name.ShouldBe("Website Redesign");
        result.Name.ShouldBe("Website Redesign");
    }

    [Fact]
    public async Task CreateAsync_WithWhitespaceName_ThrowsDomainValidationException()
    {
        await Should.ThrowAsync<DomainValidationException>(
            () => _sut.CreateAsync(
                new CreateProjectRequest { Name = "    " },
                CancellationToken.None));

        // Nothing should reach the repository when the invariant fails.
        _projectRepository.Verify(
            repository => repository.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenProjectExists_UpdatesName()
    {
        var project = Project.Create("Old Name");
        _projectRepository
            .Setup(repository => repository.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        _projectRepository
            .Setup(repository => repository.UpdateAsync(project, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.UpdateAsync(
            project.Id,
            new UpdateProjectRequest { Name = "  New Name  " },
            CancellationToken.None);

        result.Name.ShouldBe("New Name");
        // Renaming must not re-date the project.
        result.CreatedAt.ShouldBe(project.CreatedAt);
    }

    [Fact]
    public async Task UpdateAsync_WhenProjectDoesNotExist_ThrowsProjectNotFoundException()
    {
        var missingId = Guid.CreateVersion7();
        _projectRepository
            .Setup(repository => repository.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var exception = await Should.ThrowAsync<ProjectNotFoundException>(
            () => _sut.UpdateAsync(
                missingId,
                new UpdateProjectRequest { Name = "Anything" },
                CancellationToken.None));

        exception.Message.ShouldContain(missingId.ToString());
        _projectRepository.Verify(
            repository => repository.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithWhitespaceName_ThrowsDomainValidationException()
    {
        var project = Project.Create("Old Name");
        _projectRepository
            .Setup(repository => repository.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        await Should.ThrowAsync<DomainValidationException>(
            () => _sut.UpdateAsync(
                project.Id,
                new UpdateProjectRequest { Name = "   " },
                CancellationToken.None));

        project.Name.ShouldBe("Old Name");
        _projectRepository.Verify(
            repository => repository.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
