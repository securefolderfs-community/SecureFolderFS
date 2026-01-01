using System;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class LocalizationServiceExtensions
    {
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
            var dateString = dateTime switch
            {
                _ when dateTime.Year == 1 => "Unspecified",
                _ when dateTime.Date == DateTime.Today => "DateToday".ToLocalized(localizationService, interpolate: dateTime.ToString("t", cultureInfo)),
                _ when daysAgo == 1 => "DateYesterday".ToLocalized(localizationService, interpolate: dateTime.ToString("t", cultureInfo)),
                _ when daysAgo is >= 2 and <= 6 => "DateDaysAgo".ToLocalized(localizationService, interpolate: daysAgo.ToString()),
                _ when daysAgo is >= 7 and < 14 => "DateWeekAgo".ToLocalized(localizationService),
                _ => null
            };

            dateString ??= $"{dateTime.ToString("d", cultureInfo)}, {dateTime.ToString("t", cultureInfo)}";
            return dateString;
        }
    }
}
