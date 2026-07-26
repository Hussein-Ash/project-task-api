using Moq;
using ProjectTaskApi.Application.Projects.Interfaces;
using ProjectTaskApi.Application.Tasks.Dtos;
using ProjectTaskApi.Application.Tasks.Interfaces;
using ProjectTaskApi.Application.Tasks.Services;
using ProjectTaskApi.Domain.Entities;
using ProjectTaskApi.Domain.Exceptions;
using Shouldly;

namespace ProjectTaskApi.UnitTests.Tasks;

public sealed class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepository = new(MockBehavior.Strict);
    private readonly Mock<IProjectRepository> _projectRepository = new(MockBehavior.Strict);
    private readonly TaskService _sut;

    public TaskServiceTests() =>
        _sut = new TaskService(_taskRepository.Object, _projectRepository.Object);

    [Fact]
    public async Task CreateAsync_WhenProjectExists_PersistsTask()
    {
        var projectId = Guid.CreateVersion7();
        TaskItem? persisted = null;

        _projectRepository
            .Setup(repository => repository.ExistsAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _taskRepository
            .Setup(repository => repository.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .Callback<TaskItem, CancellationToken>((task, _) => persisted = task)
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync(
            projectId,
            new CreateTaskRequest { Title = "Design homepage" },
            CancellationToken.None);

        persisted.ShouldNotBeNull();
        persisted.ProjectId.ShouldBe(projectId);
        persisted.Title.ShouldBe("Design homepage");
        result.Id.ShouldBe(persisted.Id);
        result.ProjectId.ShouldBe(projectId);
    }

    [Fact]
    public async Task CreateAsync_WhenProjectDoesNotExist_ThrowsProjectNotFoundException()
    {
        var missingProjectId = Guid.CreateVersion7();
        _projectRepository
            .Setup(repository => repository.ExistsAsync(missingProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var exception = await Should.ThrowAsync<ProjectNotFoundException>(
            () => _sut.CreateAsync(
                missingProjectId,
                new CreateTaskRequest { Title = "Design homepage" },
                CancellationToken.None));

        exception.Message.ShouldContain(missingProjectId.ToString());
        // The task must never be attempted against a project that does not exist.
        _taskRepository.Verify(
            repository => repository.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_SetsCompletedToFalse()
    {
        var projectId = Guid.CreateVersion7();
        _projectRepository
            .Setup(repository => repository.ExistsAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _taskRepository
            .Setup(repository => repository.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync(
            projectId,
            new CreateTaskRequest { Title = "Design homepage" },
            CancellationToken.None);

        result.Completed.ShouldBeFalse();
        // A task that has never been completed carries no completion time.
        result.CompletedAt.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskExists_UpdatesTitleAndCompleted()
    {
        var projectId = Guid.CreateVersion7();
        var task = TaskItem.Create(projectId, "Design homepage");

        _taskRepository
            .Setup(repository => repository.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _taskRepository
            .Setup(repository => repository.UpdateAsync(task, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.UpdateAsync(
            task.Id,
            new UpdateTaskRequest { Title = "Design homepage v2", Completed = true },
            CancellationToken.None);

        result.Title.ShouldBe("Design homepage v2");
        result.Completed.ShouldBeTrue();
        // A task cannot change projects, so the id must survive the update untouched.
        result.ProjectId.ShouldBe(projectId);
    }

    [Fact]
    public async Task UpdateAsync_WhenCompleting_StampsCompletedAt()
    {
        var task = TaskItem.Create(Guid.CreateVersion7(), "Design homepage");
        task.CompletedAt.ShouldBeNull();

        _taskRepository
            .Setup(repository => repository.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _taskRepository
            .Setup(repository => repository.UpdateAsync(task, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.UpdateAsync(
            task.Id,
            new UpdateTaskRequest { Title = "Design homepage", Completed = true },
            CancellationToken.None);

        result.CompletedAt.ShouldNotBeNull();
        result.CompletedAt.Value.ShouldBeGreaterThanOrEqualTo(result.CreatedAt);
    }

    [Fact]
    public async Task UpdateAsync_WhenAlreadyCompleted_KeepsOriginalCompletedAt()
    {
        var task = TestEntities.CompletedTask(Guid.CreateVersion7(), "Ship it");
        var originalCompletedAt = task.CompletedAt;

        _taskRepository
            .Setup(repository => repository.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _taskRepository
            .Setup(repository => repository.UpdateAsync(task, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.UpdateAsync(
            task.Id,
            new UpdateTaskRequest { Title = "Ship it now", Completed = true },
            CancellationToken.None);

        // CompletedAt records when the task first became complete, not when it was last saved.
        result.CompletedAt.ShouldBe(originalCompletedAt);
        result.Title.ShouldBe("Ship it now");
    }

    [Fact]
    public async Task UpdateAsync_WhenReopening_ClearsCompletedAt()
    {
        var task = TestEntities.CompletedTask(Guid.CreateVersion7(), "Ship it");
        task.CompletedAt.ShouldNotBeNull();

        _taskRepository
            .Setup(repository => repository.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _taskRepository
            .Setup(repository => repository.UpdateAsync(task, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.UpdateAsync(
            task.Id,
            new UpdateTaskRequest { Title = "Ship it", Completed = false },
            CancellationToken.None);

        result.Completed.ShouldBeFalse();
        // A stale completion time on an incomplete task would contradict the flag.
        result.CompletedAt.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskDoesNotExist_ThrowsTaskNotFoundException()
    {
        var missingId = Guid.CreateVersion7();
        _taskRepository
            .Setup(repository => repository.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var exception = await Should.ThrowAsync<TaskNotFoundException>(
            () => _sut.UpdateAsync(
                missingId,
                new UpdateTaskRequest { Title = "Anything", Completed = false },
                CancellationToken.None));

        exception.Message.ShouldContain(missingId.ToString());
    }

    [Fact]
    public async Task DeleteAsync_WhenTaskExists_RemovesTask()
    {
        var task = TaskItem.Create(Guid.CreateVersion7(), "Design homepage");

        _taskRepository
            .Setup(repository => repository.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _taskRepository
            .Setup(repository => repository.DeleteAsync(task, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.DeleteAsync(task.Id, CancellationToken.None);

        _taskRepository.Verify(
            repository => repository.DeleteAsync(task, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenTaskDoesNotExist_ThrowsTaskNotFoundException()
    {
        var missingId = Guid.CreateVersion7();
        _taskRepository
            .Setup(repository => repository.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var exception = await Should.ThrowAsync<TaskNotFoundException>(
            () => _sut.DeleteAsync(missingId, CancellationToken.None));

        exception.Message.ShouldContain(missingId.ToString());
        _taskRepository.Verify(
            repository => repository.DeleteAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
