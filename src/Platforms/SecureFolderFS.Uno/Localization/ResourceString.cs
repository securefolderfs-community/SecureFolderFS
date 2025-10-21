using System.ComponentModel;
using Microsoft.UI.Xaml.Markup;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;

namespace SecureFolderFS.Uno.Localization
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [MarkupExtensionReturnType(ReturnType = typeof(string))]
    internal sealed class ResourceString : MarkupExtension
    {
        private static ILocalizationService? LocalizationService { get; set; }

        /// <summary>
        /// Gets or sets the name identifier associated with a resource.
        /// </summary>
        public string? Rid { get; set; }

        /// <inheritdoc/>
        protected override object ProvideValue()
        {
            LocalizationService ??= DI.OptionalService<ILocalizationService>();
            if (LocalizationService is null)
                return $"{{{Rid}}}";

            return LocalizationService.GetResource(Rid ?? string.Empty) ?? $"{{{Rid}}}";
        }
    }
}
