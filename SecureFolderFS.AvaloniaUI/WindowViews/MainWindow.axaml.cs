using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentAvalonia.UI.Windowing;
using SecureFolderFS.AvaloniaUI.Helpers;
using SecureFolderFS.AvaloniaUI.Services;
using SecureFolderFS.Sdk.Services.UserPreferences;

namespace SecureFolderFS.AvaloniaUI.WindowViews
{
    public sealed partial class MainWindow : AppWindow
    {
        private bool _hasBackgroundChangedAfterThemeChange = true;

#nullable disable
        public static MainWindow Instance { get; private set; }
#nullable enable

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();

            EnsureEarlyWindow();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == BackgroundProperty)
                _hasBackgroundChangedAfterThemeChange = true;

            base.OnPropertyChanged(change);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void EnsureEarlyWindow()
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, build: 22000))
                return;

            // Extend title bar
            TitleBar.ExtendsContentIntoTitleBar = true;

            // Set window buttons background to transparent
            TitleBar.ButtonBackgroundColor = Colors.Transparent;
            TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            TitleBar.ButtonInactiveForegroundColor = Colors.DimGray;

            IsCustomTitleBarVisible = true;

            // Set font
            FontFamily = FontFamily.Parse("Segoe UI Variable");
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                { "ContentControlThemeFontFamily", FontFamily }
            });

            // Use Mica
            TransparencyLevelHint = WindowTransparencyLevel.Mica;
            PrepareBackgroundForMica();
            ThemeHelper.Instance.OnThemeChangedEvent += OnApplicationThemeChanged;
        }

        private void OnApplicationThemeChanged(object? sender, EventArgs e)
        {
            PrepareBackgroundForMica();
        }

        private void PrepareBackgroundForMica()
        {
            if (_hasBackgroundChangedAfterThemeChange)
            {
                _hasBackgroundChangedAfterThemeChange = false;

                var background = (SolidColorBrush)Background;
                background.Opacity = 0.8d;

                // Darken background to make up for the lesser opacity
                background.Color = Color.FromArgb(background.Color.A, (byte)(background.Color.R - 4), (byte)(background.Color.G - 4), (byte)(background.Color.B - 4));
            }
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