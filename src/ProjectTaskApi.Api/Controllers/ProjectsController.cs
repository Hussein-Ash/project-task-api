using Microsoft.AspNetCore.Mvc;
using ProjectTaskApi.Application.Common;
using ProjectTaskApi.Application.Projects;
using ProjectTaskApi.Application.Projects.Dtos;
using ProjectTaskApi.Application.Tasks;
using ProjectTaskApi.Application.Tasks.Dtos;

namespace ProjectTaskApi.Api.Controllers;

/// <summary>
/// Routes match the brief exactly, with no <c>/api</c> prefix.
/// </summary>
[ApiController]
[Route("projects")]
[Produces("application/json")]
public sealed class ProjectsController(
    IProjectService projectService,
    ITaskService taskService) : ControllerBase
{
    /// <summary>Returns projects newest first, paginated.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ProjectResponse>>> GetPaged(
        [FromQuery] PageRequest request,
        CancellationToken cancellationToken) =>
        Ok(await projectService.GetPagedAsync(request, cancellationToken));

    /// <summary>Returns a project with its tasks, optionally filtered by completion state.</summary>
    [HttpGet("{id:guid}", Name = nameof(GetProjectById))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailResponse>> GetProjectById(
        Guid id,
        [FromQuery] bool? completed,
        CancellationToken cancellationToken) =>
        Ok(await projectService.GetByIdAsync(id, completed, cancellationToken));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProjectResponse>> Create(
        CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var project = await projectService.CreateAsync(request, cancellationToken);

        return CreatedAtRoute(nameof(GetProjectById), new { id = project.Id }, project);
    }

    /// <summary>
    /// Creates a task for a project. Lives here rather than on the tasks controller because
    /// the route hangs off <c>/projects</c>.
    /// </summary>
    [HttpPost("{id:guid}/tasks")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> CreateTask(
        Guid id,
        CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var task = await taskService.CreateAsync(id, request, cancellationToken);

        // There is no GET /tasks/{id} in the brief, so the Location header is built directly.
        return Created($"/tasks/{task.Id}", task);
    }
}
