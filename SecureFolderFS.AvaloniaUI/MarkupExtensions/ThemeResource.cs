using System;
using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.Styling;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.Helpers;
using SecureFolderFS.UI.Enums;

namespace SecureFolderFS.AvaloniaUI.MarkupExtensions
{
    [ObservableObject]
    internal sealed partial class ThemeResource : MarkupExtension
    {
        /// <summary>
        /// Gets the currently provided value.
        /// </summary>
        public object? Value
        {
            get
            {
                var theme = AvaloniaLocator.Current.GetRequiredService<FluentAvaloniaTheme>();
                return theme.RequestedTheme == FluentAvaloniaTheme.LightModeString ? LightValue : DarkValue;
            }
        }

        /// <summary>
        /// Gets or sets the value to provide when the application's theme is light.
        /// </summary>
        public object? LightValue { get; set; }
        
        /// <summary>
        /// Gets or sets the value to provide when the application's theme is dark.
        /// </summary>
        public object? DarkValue { get; set; }

        public ThemeResource()
        {
            WeakEventHandlerManager.Subscribe<ThemeHelper, GenericEventArgs<ApplicationTheme>, ThemeResource>(ThemeHelper.Instance, nameof(ThemeHelper.Instance.OnThemeChangedEvent), OnThemeChanged);
            ThemeHelper.Instance.OnThemeChangedEvent += OnThemeChanged;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new Binding
            {
                Source = this,
                Path = nameof(Value)
            };
        }

        private void OnThemeChanged(object? sender, GenericEventArgs<ApplicationTheme> e)
        {
            OnPropertyChanged(nameof(Value));
        }
    }
}