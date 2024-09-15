using SecureFolderFS.Sdk.Extensions;
using System;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.UI.ValueConverters
{
    public abstract class BaseDateTimeToStringConverter : BaseConverter
    {
        /// <inheritdoc/>
        protected override object? TryConvert(object? value, Type targetType, object? parameter)
        {
            if (value is not DateTime dateTime)
                return string.Empty;
            
            var cultureInfo = DI.Service<ILocalizationService>().CurrentCulture;
            var dateString = dateTime switch
            {
                _ when dateTime.Year == 1 => "Unspecified",
                _ when dateTime.Date == DateTime.Today => SafetyHelpers.NoThrowResult(() => string.Format("DateToday".ToLocalized(), dateTime.ToString("t", cultureInfo))),
                _ => null
            };

            dateString ??= $"{dateTime.ToString("d", cultureInfo)}, {dateTime.ToString("t", cultureInfo)}";
            if (parameter is string formatString)
            {
                var split = formatString.Split('|');
                if (split[0] == "LOCALIZE")
                    return string.Format(split[1].ToLocalized(), dateString);
                else
                    return string.Format(split[1], dateString);
            }

            return dateString;
        }

        /// <inheritdoc/>
        protected override object? TryConvertBack(object? value, Type targetType, object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}
