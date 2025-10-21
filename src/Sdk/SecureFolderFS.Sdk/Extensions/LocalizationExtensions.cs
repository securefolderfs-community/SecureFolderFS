using System.Runtime.CompilerServices;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Helpers;

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
            return localizationService?.GetResource(resourceKey) ?? $"{{{resourceKey}}}";

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static ILocalizationService? GetLocalizationService(ILocalizationService? fallback)
            {
                return _fallbackLocalizationService ??= fallback ?? DI.OptionalService<ILocalizationService>();
            }
        }

        /// <summary>
        /// Converts the specified resource key to its localized string representation using the provided localization service
        /// and formats it with the provided interpolation parameters.
        /// </summary>
        /// <param name="resourceKey">The key representing the resource to be localized and formatted.</param>
        /// <param name="interpolate">An array of parameters used for string interpolation in the localized string representation.</param>
        /// <returns>A formatted and localized string corresponding to the specified resource key. If localization or formatting fails, the resource key surrounded by curly braces ("{...}") is returned.</returns>
        public static string ToLocalized(this string resourceKey, params object?[] interpolate)
        {
            var localized = ToLocalized(resourceKey);
            return SafetyHelpers.NoFailureResult(() => string.Format(localized, interpolate)) ?? localized;
        }
    }
}
