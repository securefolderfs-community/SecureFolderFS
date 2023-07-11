using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Markup;
using SecureFolderFS.Sdk.Services;
using System.ComponentModel;

namespace SecureFolderFS.WinUI.Localization
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [MarkupExtensionReturnType(ReturnType = typeof(string))]
    internal sealed class ResourceString : MarkupExtension
    {
        private static ILocalizationService? LocalizationService { get; set; }

        /// <summary>
        /// Gets or sets the name identifier that is associated with a resource.
        /// </summary>
        public string? Name { get; set; }

        /// <inheritdoc/>
        protected override object ProvideValue()
        {
            LocalizationService ??= Ioc.Default.GetService<ILocalizationService>();
            if (LocalizationService is null)
                return $"{{{Name}}}";

            return LocalizationService.TryGetString(Name ?? string.Empty) ?? $"{{{Name}}}";
        }
    }
}
