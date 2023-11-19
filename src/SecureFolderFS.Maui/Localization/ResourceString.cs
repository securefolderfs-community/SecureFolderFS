using SecureFolderFS.Sdk.Services;
using System.ComponentModel;

namespace SecureFolderFS.Maui.Localization
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class ResourceString : IMarkupExtension<string>
    {
        private static ILocalizationService? LocalizationService { get; set; }

        /// <summary>
        /// Gets or sets the name identifier that is associated with a resource.
        /// </summary>
        public string? Name { get; set; }

        /// <inheritdoc/>
        public string ProvideValue(IServiceProvider serviceProvider)
        {
            return Name ?? "NoString";

            //LocalizationService ??= Ioc.Default.GetService<ILocalizationService>();
            //if (LocalizationService is null)
            //    return $"{{{Name}}}";

            //return LocalizationService.TryGetString(Name ?? string.Empty) ?? $"{{{Name}}}";
        }

        /// <inheritdoc/>
        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValue(serviceProvider);
        }
    }
}
