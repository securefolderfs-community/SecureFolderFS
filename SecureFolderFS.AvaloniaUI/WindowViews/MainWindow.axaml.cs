using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
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
            Instance = this;
            InitializeComponent();

            EnsureEarlyWindow();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void EnsureEarlyWindow()
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(10, build: 22000))
                return;

            // Extend title bar
            TitleBar.ExtendsContentIntoTitleBar = true;

            TitleBar.ButtonBackgroundColor = Colors.Transparent;
            TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            IsCustomTitleBarVisible = true;
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

        public static readonly StyledProperty<bool> IsCustomTitleBarVisibleProperty =
            AvaloniaProperty.Register<MainWindow, bool>(nameof(IsCustomTitleBarVisible));

        public bool IsCustomTitleBarVisible
        {
            get => GetValue(IsCustomTitleBarVisibleProperty);
            set => SetValue(IsCustomTitleBarVisibleProperty, value);
        }
    }
}