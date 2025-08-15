using System.Runtime.CompilerServices;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class LocalizationExtensions
    {
        private static ILocalizationService? _fallbackLocalizationService;

        /// <summary>
        /// Converts the specified resource key to its localized string representation using the provided localization service.
        /// If no localization service is provided, a fallback service is used.
        /// </summary>
        /// <param name="resourceKey">The key representing the resource to be localized.</param>
        /// <param name="localizationService">An optional instance of <see cref="ILocalizationService"/> used to localize the resource key. If null, a fallback service will be used.</param>
        /// <returns>A localized string corresponding to the specified resource key. If localization fails, the resource key surrounded by curly braces ("{...}") is returned.</returns>
        public static string ToLocalized(this string resourceKey, ILocalizationService? localizationService = null)
        {
            localizationService = GetLocalizationService(localizationService);
            return localizationService?.TryGetString(resourceKey) ?? $"{{{resourceKey}}}";

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static ILocalizationService? GetLocalizationService(ILocalizationService? fallback)
            {
                return _fallbackLocalizationService ??= fallback ?? DI.OptionalService<ILocalizationService>();
            }
        }
    }
}
