using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;

namespace SecureFolderFS.Maui.Localization
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AcceptEmptyServiceProvider]
    internal sealed class ResourceString : IMarkupExtension
    {
        private static ILocalizationService? LocalizationService { get; set; }

        /// <summary>
        /// Gets or sets the name identifier associated with a resource.
        /// </summary>
        public string? Rid { get; set; }

        /// <summary>
        /// Gets or sets the converter used to transform the localized string or resource key value.
        /// </summary>
        public IValueConverter? Converter { get; set; }

        /// <summary>
        /// Gets or sets the parameter passed to the converter to provide additional context or data for conversion.
        /// </summary>
        public object? ConverterParameter { get; set; }

        /// <inheritdoc/>
        public object? ProvideValue(IServiceProvider serviceProvider)
        {
            if (Converter is null)
                return GetLocalized();

            return Converter.Convert(GetLocalized(), typeof(string), ConverterParameter, CultureInfo.CurrentCulture);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetLocalized()
        {
            LocalizationService ??= DI.OptionalService<ILocalizationService>();
            if (LocalizationService is null)
                return $"{{{Rid}}}";

            return (Rid ?? string.Empty).ToLocalized();
        }
    }
}
