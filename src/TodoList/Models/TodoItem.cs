namespace TodoList.Models;

public enum TodoStatus
{
    /// <summary>未完成</summary>
    Pending = 0,

    /// <summary>已完成</summary>
    Completed = 1,

    /// <summary>不需要完成</summary>
    Skipped = 2
}

public class TodoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>工作内容</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>状态</summary>
    public TodoStatus Status { get; set; } = TodoStatus.Pending;

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>截止时间（可选）</summary>
    public DateTime? DueTime { get; set; }

    /// <summary>时间范围 - 开始（可选）</summary>
    public DateTime? StartTime { get; set; }

    /// <summary>时间范围 - 结束（可选）</summary>
    public DateTime? EndTime { get; set; }
}
