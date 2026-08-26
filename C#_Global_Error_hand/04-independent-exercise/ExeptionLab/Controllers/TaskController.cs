using TaskManeger.Models;
using TaskManeger.Repositories;
using Microsoft.AspNetCore.Mvc;
namespace TaskManeger.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    private readonly ITaskManeger _taskManager;
    public TaskController(ITaskManeger taskManager)
    {
        _taskManager = taskManager;
    }
    [HttpGet]
    public async Task<ActionResult<List<TaskItem>>> GetAllTasks()
    {
        var tasks = await _taskManager.GetAllTasksAsync();
        return Ok(tasks);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetTaskById(int id)
    {
        var task = await _taskManager.GetTaskByIdAsync(id);
        if (task == null)
        {
            return NotFound($"Task with id {id} was not found.");
        }
        return Ok(task);
    }
    [HttpGet("status")]
    public async Task<ActionResult<List<TaskItem>>> GetTasksByStatus([FromQuery] string status)
    {
        if (status != "completed" && status != "pending")
        {
            return BadRequest("Status must be 'completed' or 'pending'.");
        }

        var tasks = await _taskManager.GetTaskByStatusAsync(status);
        return Ok(tasks);
    }
    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTask(TaskItem taskItem)
    {
        var newTask = await _taskManager.CreateNewTaskAsync(taskItem);
        return CreatedAtAction(nameof(GetTaskById),new { id = newTask.Id },newTask);
    }
    [HttpPut("{id}/status")]
    public async Task<ActionResult<TaskItem>> UpdateStatus(int id)
    {
        var task = await _taskManager.UpdateStatusAsync(id);

        if (task == null)
        {
            return NotFound($"Task with id {id} was not found.");
        }
        return Ok(task);
    }
}