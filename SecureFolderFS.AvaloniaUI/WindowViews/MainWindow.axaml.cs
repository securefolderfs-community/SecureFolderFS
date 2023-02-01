using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentAvalonia.UI.Windowing;
using SecureFolderFS.AvaloniaUI.Services;
using SecureFolderFS.Sdk.Services.UserPreferences;

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
            var applicationSettingsService = Ioc.Default.GetRequiredService<IApplicationSettingsService>();
            var platformSettingsService = Ioc.Default.GetRequiredService<IPlatformSettingsService>();

            await Task.WhenAll(
                settingsService.SaveSettingsAsync(),
                applicationSettingsService.SaveSettingsAsync(),
                platformSettingsService.SaveSettingsAsync()
            );
        }
    }
}