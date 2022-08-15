using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Sdk
{
    public static class LocalizationExtensions
    {
        private static ILocalizationService? FallbackLocalizationService;

        public static string ToLocalized(this string resourceKey, ILocalizationService? localizationService = null)
        {
            localizationService = GetLocalizationService(localizationService);
            return localizationService?.LocalizeString(resourceKey) ?? string.Empty;
        }

        private static ILocalizationService? GetLocalizationService(ILocalizationService? fallback)
        {
            FallbackLocalizationService ??= fallback ?? Ioc.Default.GetService<ILocalizationService>();
            return FallbackLocalizationService;
        }
    }
}
