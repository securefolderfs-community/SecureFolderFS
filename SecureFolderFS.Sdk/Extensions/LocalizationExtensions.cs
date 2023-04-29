using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class LocalizationExtensions
    {
        private static ILocalizationService? FallbackLocalizationService;

        public static string ToLocalized(this string resourceKey, ILocalizationService? localizationService = null)
        {
            localizationService = GetLocalizationService(localizationService);
            return localizationService?.GetString(resourceKey) ?? string.Empty;
        }

        private static ILocalizationService? GetLocalizationService(ILocalizationService? fallback)
        {
            FallbackLocalizationService ??= fallback ?? Ioc.Default.GetService<ILocalizationService>();
            return FallbackLocalizationService;
        }
    }
}
