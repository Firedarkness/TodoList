using Avalonia.Controls;
using Avalonia.Interactivity;
using TodoList.ViewModels;

namespace TodoList.Views;

public partial class TodoEditWindow : Window
{
    public TodoEditWindow()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    private void Save_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TodoEditViewModel vm && vm.TrySave())
        {
            Close(vm.Result);
        }
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e) => Close(null);
}
