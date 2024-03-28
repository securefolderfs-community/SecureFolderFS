using System.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services;

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
            LocalizationService ??= Ioc.Default.GetService<ILocalizationService>();
            if (LocalizationService is null)
                return $"{{{Name}}}";

            return LocalizationService.TryGetString(Name ?? string.Empty) ?? $"{{{Name}}}";
        }
    }
}
