using System;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TodoList.Models;

namespace TodoList.ViewModels;

/// <summary>
/// 单条 TODO 的展示模型：包装 <see cref="TodoItem"/>，
/// 状态/时间变化时同步回 Model 并通知 UI。
/// </summary>
public partial class TodoItemViewModel : ViewModelBase
{
    private static readonly IBrush PendingBrush = new ImmutableSolidColorBrush(Color.Parse("#0F6CBD"));
    private static readonly IBrush CompletedBrush = new ImmutableSolidColorBrush(Color.Parse("#0F7B4F"));
    private static readonly IBrush SkippedBrush = new ImmutableSolidColorBrush(Color.Parse("#9E9E9E"));
    private static readonly IBrush OverdueBrush = new ImmutableSolidColorBrush(Color.Parse("#D13438"));
    private static readonly IBrush SecondaryTextBrush = new ImmutableSolidColorBrush(Color.Parse("#757575"));
    private static readonly IBrush DangerBrush = new ImmutableSolidColorBrush(Color.Parse("#C42B1C"));
    private static readonly IBrush BorderGrayBrush = new ImmutableSolidColorBrush(Color.Parse("#D0D0D0"));

    public Guid Id { get; }
    public DateTime CreatedAt { get; }
    public TodoItem Model { get; }

    /// <summary>状态变化（已同步回 Model）后触发，供主界面重新排序保存。</summary>
    public event Action? StatusChanged;

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private TodoStatus _status;

    [ObservableProperty]
    private DateTime? _dueTime;

    [ObservableProperty]
    private DateTime? _startTime;

    [ObservableProperty]
    private DateTime? _endTime;

    [ObservableProperty]
    private bool _isDeleteConfirming;

    public TodoItemViewModel(TodoItem model)
    {
        Model = model;
        Id = model.Id;
        CreatedAt = model.CreatedAt;
        _content = model.Content;
        _status = model.Status;
        _dueTime = model.DueTime;
        _startTime = model.StartTime;
        _endTime = model.EndTime;
    }

    public string StatusText => Status switch
    {
        TodoStatus.Pending => "未完成",
        TodoStatus.Completed => "已完成",
        TodoStatus.Skipped => "不需要完成",
        _ => "未知"
    };

    public string StatusSymbol => Status switch
    {
        TodoStatus.Pending => "○",
        TodoStatus.Completed => "✓",
        TodoStatus.Skipped => "✕",
        _ => "?"
    };

    public IBrush StatusBrush => Status switch
    {
        TodoStatus.Completed => CompletedBrush,
        TodoStatus.Skipped => SkippedBrush,
        _ => PendingBrush
    };

    public bool IsOverdue =>
        Status == TodoStatus.Pending && DueTime is { } due && due < DateTime.Now;

    public string DueText => DueTime is { } d ? $"截止 {d:yyyy-MM-dd HH:mm}" : string.Empty;

    public IBrush DueBrush => IsOverdue ? OverdueBrush : SecondaryTextBrush;

    public bool HasDue => DueTime.HasValue;

    public string RangeText
    {
        get
        {
            if (StartTime is not { } s)
                return EndTime is { } e ? $"至 {e:yyyy-MM-dd HH:mm}" : string.Empty;
            return EndTime is { } e2
                ? $"{s:MM-dd HH:mm} ~ {e2:MM-dd HH:mm}"
                : $"{s:yyyy-MM-dd HH:mm} 起";
        }
    }

    public bool HasRange => StartTime.HasValue || EndTime.HasValue;

    public string CreatedText => $"创建 {CreatedAt:yyyy-MM-dd HH:mm}";

    public TextDecorationCollection? ContentDecorations =>
        Status == TodoStatus.Completed ? TextDecorations.Strikethrough : null;

    public double CardOpacity => Status switch
    {
        TodoStatus.Completed => 0.75,
        TodoStatus.Skipped => 0.55,
        _ => 1.0
    };

    public IBrush DeleteBackground => IsDeleteConfirming ? DangerBrush : Brushes.Transparent;
    public IBrush DeleteForeground => IsDeleteConfirming ? Brushes.White : SecondaryTextBrush;
    public IBrush DeleteBorderBrush => IsDeleteConfirming ? DangerBrush : BorderGrayBrush;

    partial void OnStatusChanged(TodoStatus value)
    {
        Model.Status = value; // 同步回模型，供持久化

        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusSymbol));
        OnPropertyChanged(nameof(StatusBrush));
        OnPropertyChanged(nameof(IsOverdue));
        OnPropertyChanged(nameof(DueBrush));
        OnPropertyChanged(nameof(ContentDecorations));
        OnPropertyChanged(nameof(CardOpacity));

        StatusChanged?.Invoke();
    }

    partial void OnDueTimeChanged(DateTime? value)
    {
        OnPropertyChanged(nameof(DueText));
        OnPropertyChanged(nameof(HasDue));
        OnPropertyChanged(nameof(IsOverdue));
        OnPropertyChanged(nameof(DueBrush));
    }

    partial void OnStartTimeChanged(DateTime? value)
    {
        OnPropertyChanged(nameof(RangeText));
        OnPropertyChanged(nameof(HasRange));
    }

    partial void OnEndTimeChanged(DateTime? value)
    {
        OnPropertyChanged(nameof(RangeText));
        OnPropertyChanged(nameof(HasRange));
    }

    partial void OnIsDeleteConfirmingChanged(bool value)
    {
        OnPropertyChanged(nameof(DeleteBackground));
        OnPropertyChanged(nameof(DeleteForeground));
        OnPropertyChanged(nameof(DeleteBorderBrush));
    }

    /// <summary>点击状态图标：未完成 → 已完成 → 不需要完成 → 未完成。</summary>
    [RelayCommand]
    private void CycleStatus()
    {
        Status = Status switch
        {
            TodoStatus.Pending => TodoStatus.Completed,
            TodoStatus.Completed => TodoStatus.Skipped,
            _ => TodoStatus.Pending
        };
    }
}
