using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TodoList.Models;
using TodoList.Services;
using TodoList.Views;

namespace TodoList.ViewModels;

/// <summary>
/// 主界面视图模型。
/// 排序规则：未完成 → 已完成 → 不需要完成；同组内按创建时间倒序（新的在前）。
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly Window _owner;
    private readonly TodoRepository _repository;
    private readonly List<TodoItem> _items = new();

    public ObservableCollection<TodoItemViewModel> Todos { get; } = new();

    [ObservableProperty]
    private string _summaryText = "未完成 0 · 已完成 0 · 不需要完成 0";

    [ObservableProperty]
    private bool _hasNoTodos = true;

    public MainWindowViewModel(Window owner, TodoRepository repository)
    {
        _owner = owner;
        _repository = repository;
        _items.AddRange(repository.Load());
        RefreshList();
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        var item = await OpenEditDialogAsync(null);
        if (item is null)
            return;

        _items.Add(item);
        PersistAndRefresh();
    }

    [RelayCommand]
    private async Task EditAsync(TodoItemViewModel vm)
    {
        if (vm is null)
            return;

        // 编辑返回的是同一个 Model 实例（就地修改）
        var result = await OpenEditDialogAsync(vm.Model);
        if (result is null)
            return;

        PersistAndRefresh();
    }

    /// <summary>
    /// 删除：第一次点击进入“确认删除”状态（3 秒后自动复位），再次点击才真正删除。
    /// </summary>
    [RelayCommand]
    private void RequestDelete(TodoItemViewModel vm)
    {
        if (vm is null)
            return;

        if (!vm.IsDeleteConfirming)
        {
            vm.IsDeleteConfirming = true;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            timer.Tick += (_, _) =>
            {
                vm.IsDeleteConfirming = false;
                timer.Stop();
            };
            timer.Start();
            return;
        }

        _items.RemoveAll(t => t.Id == vm.Id);
        PersistAndRefresh();
    }

    private async Task<TodoItem?> OpenEditDialogAsync(TodoItem? existing)
    {
        var vm = new TodoEditViewModel(existing);
        var dialog = new TodoEditWindow { DataContext = vm };
        return await dialog.ShowDialog<TodoItem?>(_owner);
    }

    private void PersistAndRefresh()
    {
        _repository.Save(_items);
        RefreshList();
    }

    private void RefreshList()
    {
        var ordered = _items
            .OrderBy(t => (int)t.Status)          // 未完成(0) → 已完成(1) → 不需要完成(2)
            .ThenByDescending(t => t.CreatedAt)    // 同组内创建时间倒序
            .ToList();

        Todos.Clear();
        foreach (var model in ordered)
        {
            var vm = new TodoItemViewModel(model);
            vm.StatusChanged += PersistAndRefresh; // 状态切换后自动保存并重排
            Todos.Add(vm);
        }

        SummaryText = $"未完成 {CountOf(TodoStatus.Pending)} · " +
                      $"已完成 {CountOf(TodoStatus.Completed)} · " +
                      $"不需要完成 {CountOf(TodoStatus.Skipped)}";
        HasNoTodos = _items.Count == 0;
    }

    private int CountOf(TodoStatus status) => _items.Count(t => t.Status == status);
}
