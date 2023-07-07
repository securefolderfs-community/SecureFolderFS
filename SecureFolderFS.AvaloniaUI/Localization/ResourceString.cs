using System;
using System.ComponentModel;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.AvaloniaUI.Localization
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class ResourceString : MarkupExtension
    {
        private static ILocalizationService? LocalizationService { get; set; }
        
        /// <summary>
        /// Gets or sets the name identifier that is associated with a resource.
        /// </summary>
        public string Name { get; set; }

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            LocalizationService ??= Ioc.Default.GetService<ILocalizationService>();
            if (LocalizationService is null)
                return $"{{{Name}}}";

            return LocalizationService.GetString(Name) ?? $"{{{Name}}}";
        }
    }
}