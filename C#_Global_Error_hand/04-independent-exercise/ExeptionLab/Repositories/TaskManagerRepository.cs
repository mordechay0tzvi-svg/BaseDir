using TaskManeger.Models;
namespace TaskManeger.Repositories;
public class TaskManegerRepo : ITaskManeger
{
    public int _nextId = 4;
    private readonly List<TaskItem> _tasks = new();
    public TaskManegerRepo()
    {
        _tasks.Add(new TaskItem { Id = 1, Title = "Learn C#", Description = "Complete C# basics", IsCompleted = true, CreatedAt = DateTime.Now.AddDays(-5) });
        _tasks.Add(new TaskItem { Id = 2, Title = "Build API", Description = "Create REST API", IsCompleted = false, CreatedAt = DateTime.Now.AddDays(-2) });
        _tasks.Add(new TaskItem { Id = 3, Title = "Deploy", Description = "Deploy to production", IsCompleted = false, CreatedAt = DateTime.Now.AddDays(-1)});
    }
    public async Task<List<TaskItem>> GetAllTasksAsync()
    {
        return _tasks.ToList();
    }
    public async Task<TaskItem?> GetTaskByIdAsync(int id)
    {
        return _tasks.FirstOrDefault(t => t.Id == id);
    }
    public async Task<List<TaskItem>> GetTaskByStatusAsync(string status)
    {
         List<TaskItem> fit = new();
        if (status == "completed")
        {
            fit = _tasks.Where(t => t.IsCompleted == true).ToList();
        }
        if (status == "pending")
        {
            fit = _tasks.Where(t => t.IsCompleted == false).ToList();
        }
        return fit;
    }
    public async Task<TaskItem> CreateNewTaskAsync(TaskItem taskItem)
    {
        taskItem.Id = _nextId ++;
        _tasks.Add(taskItem);
        return taskItem;
    }
    public async Task<TaskItem?> UpdateStatusAsync(int id)
    {
        var update = _tasks.FirstOrDefault(t => t.Id == id);
        if (update != null)
        {
            update.IsCompleted = true;
            return update;
        }
        return null;
    }
}