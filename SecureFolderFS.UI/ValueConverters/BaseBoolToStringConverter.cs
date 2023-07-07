using SecureFolderFS.Sdk.Extensions;
using System;

namespace SecureFolderFS.UI.ValueConverters
{
    public abstract class BaseBoolToStringConverter : BaseConverter
    {
        /// <inheritdoc/>
        protected override object? TryConvert(object? value, Type targetType, object? parameter)
        {
            if (value is not bool bValue)
                return string.Empty;

            if (parameter is not string formatString)
                return bValue.ToString();

            // Example:
            // If false, localize using "StringRes" resource. If true, don't localize, just use "Some string"
            // false:LOCALIZE|StringRes:true:STANDARD|Some string

            var valueSplit = formatString.Split(':');
            var splitOption = bValue ? valueSplit[3].Split('|') : valueSplit[1].Split('|');

            return splitOption[0] == "LOCALIZE" ? splitOption[1].ToLocalized() : splitOption[1];
        }

        /// <inheritdoc/>
        protected override object? TryConvertBack(object? value, Type targetType, object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}
