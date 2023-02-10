using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentAvalonia.UI.Windowing;
using SecureFolderFS.AvaloniaUI.Services;
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
            InitializeComponent();

            Instance = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void Window_OnClosing(object? sender, CancelEventArgs e)
        {
            var settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
            await settingsService.SaveAsync();
            var platformSettingsService = Ioc.Default.GetRequiredService<IPlatformSettingsService>();
            todo
        }
    }
}