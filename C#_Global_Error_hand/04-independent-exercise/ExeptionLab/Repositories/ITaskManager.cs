using TaskManeger.Models;
namespace TaskManeger.Repositories;
public interface ITaskManeger
{
    public Task<List<TaskItem>> GetAllTasksAsync();
    public Task<TaskItem?> GetTaskByIdAsync(int id);
    public Task<List<TaskItem>> GetTaskByStatusAsync(string status);
    public Task<TaskItem> CreateNewTaskAsync(TaskItem taskItem);
    public Task<TaskItem?> UpdateStatusAsync(int id);
}