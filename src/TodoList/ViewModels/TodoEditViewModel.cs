using System;
using CommunityToolkit.Mvvm.ComponentModel;
using TodoList.Models;

namespace TodoList.ViewModels;

/// <summary>
/// 新增 / 编辑 TODO 的表单视图模型。
/// <see cref="Result"/> 为编辑结果：取消时为 null；保存时为新增对象或原对象（就地修改）。
/// </summary>
public partial class TodoEditViewModel : ViewModelBase
{
    private readonly TodoItem? _original;

    public string WindowTitle => _original is null ? "新增 TODO" : "编辑 TODO";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private string _content = string.Empty;

    /// <summary>状态下拉框索引：0=未完成 1=已完成 2=不需要完成</summary>
    [ObservableProperty]
    private int _statusIndex;

    // ---- 截止时间 ----
    [ObservableProperty]
    private bool _hasDueTime;

    [ObservableProperty]
    private DateTimeOffset _dueDate = DateTimeOffset.Now;

    [ObservableProperty]
    private TimeSpan _dueClock = new(23, 59, 0);

    // ---- 时间范围 ----
    [ObservableProperty]
    private bool _hasRange;

    [ObservableProperty]
    private DateTimeOffset _startDate = DateTimeOffset.Now;

    [ObservableProperty]
    private TimeSpan _startClock = new(9, 0, 0);

    [ObservableProperty]
    private DateTimeOffset _endDate = DateTimeOffset.Now;

    [ObservableProperty]
    private TimeSpan _endClock = new(18, 0, 0);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private string? _validationError;

    public bool CanSave => !string.IsNullOrWhiteSpace(Content) && ValidateRange() is null;

    public TodoItem? Result { get; private set; }

    public TodoEditViewModel(TodoItem? original)
    {
        _original = original;

        if (original is null)
        {
            _statusIndex = (int)TodoStatus.Pending;
            return;
        }

        _content = original.Content;
        _statusIndex = (int)original.Status;

        if (original.DueTime is { } due)
        {
            _hasDueTime = true;
            _dueDate = new DateTimeOffset(due.Date);
            _dueClock = due.TimeOfDay;
        }

        if (original.StartTime is { } start)
        {
            _hasRange = true;
            _startDate = new DateTimeOffset(start.Date);
            _startClock = start.TimeOfDay;
        }

        if (original.EndTime is { } end)
        {
            _hasRange = true;
            _endDate = new DateTimeOffset(end.Date);
            _endClock = end.TimeOfDay;
        }
    }

    partial void OnContentChanged(string value) => ValidateLive();

    partial void OnHasRangeChanged(bool value) => ValidateLive();

    partial void OnStartDateChanged(DateTimeOffset value) => ValidateLive();

    partial void OnStartClockChanged(TimeSpan value) => ValidateLive();

    partial void OnEndDateChanged(DateTimeOffset value) => ValidateLive();

    partial void OnEndClockChanged(TimeSpan value) => ValidateLive();

    private void ValidateLive()
    {
        ValidationError = string.IsNullOrWhiteSpace(Content)
            ? "工作内容不能为空"
            : ValidateRange();
    }

    private string? ValidateRange()
    {
        if (!HasRange)
            return null;

        return BuildTime(StartDate, StartClock) > BuildTime(EndDate, EndClock)
            ? "时间范围：开始时间不能晚于结束时间"
            : null;
    }

    private static DateTime BuildTime(DateTimeOffset date, TimeSpan clock) => date.Date + clock;

    /// <summary>校验并保存，成功返回 true 且 <see cref="Result"/> 就绪。</summary>
    public bool TrySave()
    {
        ValidationError = string.IsNullOrWhiteSpace(Content)
            ? "工作内容不能为空"
            : ValidateRange();

        if (ValidationError is not null)
            return false;

        var item = _original ?? new TodoItem { CreatedAt = DateTime.Now };

        item.Content = Content.Trim();
        item.Status = (TodoStatus)StatusIndex;
        item.DueTime = HasDueTime ? BuildTime(DueDate, DueClock) : null;
        item.StartTime = HasRange ? BuildTime(StartDate, StartClock) : null;
        item.EndTime = HasRange ? BuildTime(EndDate, EndClock) : null;

        Result = item;
        return true;
    }
}
