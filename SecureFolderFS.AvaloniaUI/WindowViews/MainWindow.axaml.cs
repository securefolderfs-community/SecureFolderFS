using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentAvalonia.UI.Windowing;
using SecureFolderFS.Sdk.Services;
using System.ComponentModel;

namespace SecureFolderFS.AvaloniaUI.WindowViews
{
    public sealed partial class MainWindow : AppWindow
    {
#nullable disable
        public static MainWindow Instance { get; private set; }
#nullable enable

        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
            Instance = this;
        }

        private async void Window_OnClosing(object? sender, CancelEventArgs e)
        {
            var settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
            await settingsService.SaveAsync();
        }
    }
}