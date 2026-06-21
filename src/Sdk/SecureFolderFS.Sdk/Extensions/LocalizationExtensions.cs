using System;
using System.Runtime.CompilerServices;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Helpers;
using SmartFormat;

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
            var resource = localizationService?.GetResource(resourceKey);
            if (string.IsNullOrEmpty(resource))
                return $"{{{resourceKey}}}";

            return resource.Replace("\\n", Environment.NewLine);

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
            return SafetyHelpers.NoFailureResult(() => Smart.Format(localized, interpolate)) ?? $"{{{resourceKey}}}";
        }

        /// <summary>
        /// Converts the specified resource key to its localized string representation using the provided localization service
        /// and formats it with the provided interpolation parameters.
        /// </summary>
        /// <param name="resourceKey">The key representing the resource to be localized and formatted.</param>
        /// <param name="localizationService">The <see cref="ILocalizationService"/> to use.</param>
        /// <param name="interpolate">An array of parameters used for string interpolation in the localized string representation.</param>
        /// <returns>A formatted and localized string corresponding to the specified resource key. If localization or formatting fails, the resource key surrounded by curly braces ("{...}") is returned.</returns>
        public static string ToLocalized(this string resourceKey, ILocalizationService localizationService, params object?[] interpolate)
        {
            var localized = ToLocalized(resourceKey, localizationService);
            return SafetyHelpers.NoFailureResult(() => Smart.Format(localized, interpolate)) ?? $"{{{resourceKey}}}";
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> to a localized, human-readable string representation.
        /// </summary>
        /// <param name="localizationService">The localization service used to retrieve localized strings and culture information.</param>
        /// <param name="dateTime">The date and time to localize.</param>
        /// <returns>A localized string representation of the date.</returns>
        public static string LocalizeDate(this ILocalizationService localizationService, DateTime dateTime)
        {
            var cultureInfo = localizationService.CurrentCulture;
            var daysAgo = (DateTime.Today - dateTime.Date).Days;
            var weeksAgo = daysAgo / 7;
            var dateString = dateTime switch
            {
                _ when dateTime.Year == 1 => "Unspecified",
                _ when dateTime.Date == DateTime.Today => "DateToday".ToLocalized(localizationService, interpolate: dateTime.ToString("t", cultureInfo)),
                _ when daysAgo == 1 => "DateYesterday".ToLocalized(localizationService, interpolate: dateTime.ToString("t", cultureInfo)),
                _ when daysAgo is >= 2 and <= 6 => "DateDaysAgoPlural".ToLocalized(localizationService, interpolate: daysAgo),
                _ when daysAgo is >= 7 and < 14 => "DateWeekAgoPlural".ToLocalized(localizationService, interpolate: weeksAgo),
                _ => null
            };

            dateString ??= $"{dateTime.ToString("d", cultureInfo)}, {dateTime.ToString("t", cultureInfo)}";
            return dateString;
        }
    }
}
