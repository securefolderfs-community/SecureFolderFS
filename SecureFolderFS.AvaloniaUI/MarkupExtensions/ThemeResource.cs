using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.Styling;
using SecureFolderFS.AvaloniaUI.Helpers;
using SecureFolderFS.UI.Helpers;
using System;
using System.ComponentModel;

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
            WeakEventHandlerManager.Subscribe<AvaloniaThemeHelper, PropertyChangedEventArgs, ThemeResource>(AvaloniaThemeHelper.Instance, nameof(AvaloniaThemeHelper.Instance.PropertyChanged), OnThemeChanged);
            AvaloniaThemeHelper.Instance.PropertyChanged += OnThemeChanged;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new Binding
            {
                Source = this,
                Path = nameof(Value)
            };
        }

        private void OnThemeChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IThemeHelper.CurrentTheme))
                return;

            OnPropertyChanged(nameof(Value));
        }
    }
}