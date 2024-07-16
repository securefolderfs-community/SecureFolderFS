using System.ComponentModel;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;

namespace SecureFolderFS.Maui.Localization
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class ResourceString : IMarkupExtension
    {
        private static ILocalizationService? LocalizationService { get; set; }

        /// <summary>
        /// Gets or sets the name identifier that is associated with a resource.
        /// </summary>
        public string? Name { get; set; }

        /// <inheritdoc/>
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            LocalizationService ??= DI.OptionalService<ILocalizationService>();
            if (LocalizationService is null)
                return $"{{{Name}}}";

            return LocalizationService.TryGetString(Name ?? string.Empty) ?? $"{{{Name}}}";
        }
    }
}
