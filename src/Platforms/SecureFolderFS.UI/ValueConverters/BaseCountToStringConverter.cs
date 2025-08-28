using System;
using SecureFolderFS.Sdk.Extensions;

namespace SecureFolderFS.UI.ValueConverters
{
    public abstract class BaseCountToStringConverter : BaseConverter
    {
        /// <inheritdoc/>
        protected override object? TryConvert(object? value, Type targetType, object? parameter)
        {
            if (value is not int numValue)
                return string.Empty;

            if (parameter is not string formatString)
                return numValue.ToString();

            // Example:
            // localize|StringRes
            // none|Some string {0}

            var split = formatString.Split('|', 2);
            var result = split[0].Equals("localize", StringComparison.OrdinalIgnoreCase) ? split[1].ToLocalized() : split[1];

            return string.Format(result, numValue);
        }

        /// <inheritdoc/>
        protected override object? TryConvertBack(object? value, Type targetType, object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}
