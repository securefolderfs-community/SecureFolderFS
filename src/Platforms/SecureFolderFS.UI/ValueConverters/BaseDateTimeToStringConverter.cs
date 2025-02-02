using System;
using SecureFolderFS.Sdk.Extensions;
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
            
            var localizationService = DI.Service<ILocalizationService>();
            var dateString = localizationService.LocalizeDate(dateTime);
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
