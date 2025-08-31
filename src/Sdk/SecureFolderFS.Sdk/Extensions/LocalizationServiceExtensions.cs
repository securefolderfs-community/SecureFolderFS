using System;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class LocalizationServiceExtensions
    {
        public static string LocalizeDate(this ILocalizationService localizationService, DateTime dateTime)
        {
            var cultureInfo = localizationService.CurrentCulture;
            var dateString = dateTime switch
            {
                _ when dateTime.Year == 1 => "Unspecified",
                _ when dateTime.Date == DateTime.Today => "DateToday".ToLocalized(dateTime.ToString("t", cultureInfo)),
                _ => null
            };

            dateString ??= $"{dateTime.ToString("d", cultureInfo)}, {dateTime.ToString("t", cultureInfo)}";
            return dateString;
        }
    }
}
