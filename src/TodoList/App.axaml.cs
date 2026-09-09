using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TodoList.Services;
using TodoList.ViewModels;
using TodoList.Views;

namespace TodoList;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var repository = new TodoRepository(
                Path.Combine(AppContext.BaseDirectory, "todos.json"));

            var mainWindow = new MainWindow();
            mainWindow.DataContext = new MainWindowViewModel(mainWindow, repository);
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
