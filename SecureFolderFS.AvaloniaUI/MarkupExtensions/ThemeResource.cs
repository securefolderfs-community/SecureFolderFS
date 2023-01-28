using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.Helpers;
using SecureFolderFS.UI.Enums;

namespace SecureFolderFS.AvaloniaUI.MarkupExtensions
{
    [ObservableObject]
    internal sealed partial class ThemeResource : MarkupExtension
    {
        public object? Resource
        {
            get
            {
                var theme = AvaloniaLocator.Current.GetRequiredService<FluentAvaloniaTheme>();
                return theme.RequestedTheme == FluentAvaloniaTheme.LightModeString ? LightResource : DarkResource;
            }
        }

        public object? LightResource { get; set; }
        public object? DarkResource { get; set; }

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
                Path = nameof(Resource)
            };
        }

        private void OnThemeChanged(object? sender, GenericEventArgs<ApplicationTheme> e)
        {
            OnPropertyChanged(nameof(Resource));
        }
    }
}