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

            // Examples:
            // false:localize|StringRes:true:none|Some string
            // true:none|Some string:false:localize|StringRes

            var valueSplit = formatString.Split(':');

            // Determine order by reading the first label
            var firstIsTrue = valueSplit[0].Equals("true", StringComparison.OrdinalIgnoreCase);
            var splitOption = (bValue == firstIsTrue) ? valueSplit[1].Split('|') : valueSplit[3].Split('|');

            return splitOption[0].Equals("localize", StringComparison.OrdinalIgnoreCase)
                ? splitOption[1].ToLocalized()
                : splitOption[1];
        }

        /// <inheritdoc/>
        protected override object? TryConvertBack(object? value, Type targetType, object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}
