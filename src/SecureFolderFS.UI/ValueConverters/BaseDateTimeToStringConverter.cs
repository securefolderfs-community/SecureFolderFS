using SecureFolderFS.Sdk.Extensions;
using System;

namespace SecureFolderFS.UI.ValueConverters
{
    public abstract class BaseDateTimeToStringConverter : BaseConverter
    {
        /// <inheritdoc/>
        protected override object? TryConvert(object? value, Type targetType, object? parameter)
        {
            if (value is not DateTime dateTime)
                return string.Empty;

            string dateString;
            if (dateTime.Date == DateTime.Today)
                dateString = $"Today, {dateTime.ToString("HH:mm")}"; // TODO: Localize
            else
                dateString = dateTime.Year == 1 ? "Unspecified" : dateTime.ToString("MM/dd/yyyy, HH:mm");

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
