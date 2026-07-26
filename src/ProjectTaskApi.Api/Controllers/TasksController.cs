using Microsoft.AspNetCore.Mvc;
using ProjectTaskApi.Application.Tasks.Dtos;
using ProjectTaskApi.Application.Tasks.Interfaces;

namespace ProjectTaskApi.Api.Controllers;

[ApiController]
[Route("tasks")]
[Produces("application/json")]
public sealed class TasksController(ITaskService taskService) : ControllerBase
{
    /// <summary>Replaces a task in full; both title and completed are required.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> Update(
        Guid id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken) =>
        Ok(await taskService.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await taskService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
