using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Markup;
using SecureFolderFS.Sdk.Services;
using System.ComponentModel;
using SecureFolderFS.Shared;

namespace SecureFolderFS.Uno.Localization
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
            LocalizationService ??= DI.OptionalService<ILocalizationService>();
            if (LocalizationService is null)
                return $"{{{Name}}}";

            return LocalizationService.TryGetString(Name ?? string.Empty) ?? $"{{{Name}}}";
        }
    }
}
