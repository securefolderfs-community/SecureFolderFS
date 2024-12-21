using System.Runtime.CompilerServices;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class LocalizationExtensions
    {
        private static ILocalizationService? FallbackLocalizationService;

        public static string ToLocalized(this string resourceKey, ILocalizationService? localizationService = null)
        {
            localizationService = GetLocalizationService(localizationService);
            return localizationService?.TryGetString(resourceKey) ?? $"{{{resourceKey}}}";

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static ILocalizationService? GetLocalizationService(ILocalizationService? fallback)
            {
                return FallbackLocalizationService ??= fallback ?? DI.OptionalService<ILocalizationService>();
            }
        }
    }
}
