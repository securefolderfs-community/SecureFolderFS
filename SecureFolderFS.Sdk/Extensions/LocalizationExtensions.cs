using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class LocalizationExtensions
    {
        private static ILocalizationService? FallbackLocalizationService;

        public static string ToLocalized(this string resourceKey)
        {
            return ToLocalized(resourceKey, null);
        }

        public static string ToLocalized(this string resourceKey, ILocalizationService? localizationService)
        {
            localizationService = GetLocalizationService(localizationService);
            return localizationService?.TryGetString(resourceKey) ?? $"{{{resourceKey}}}";

            static ILocalizationService? GetLocalizationService(ILocalizationService? fallback)
            {
                FallbackLocalizationService ??= fallback ?? Ioc.Default.GetService<ILocalizationService>();
                return FallbackLocalizationService;
            }
        }
    }
}
