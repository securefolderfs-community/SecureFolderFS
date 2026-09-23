using System;
using System.Globalization;

namespace SecureFolderFS.UI.ValueConverters
{
    /// <summary>
    /// Converts a boolean into one of two numbers.
    /// </summary>
    /// <remarks>The converter parameter holds both values as "whenTrue|whenFalse", e.g. "0.4|1.0".</remarks>
    public abstract class BaseBoolToDoubleConverter : BaseConverter
    {
        /// <inheritdoc/>
        protected override object? TryConvert(object? value, Type targetType, object? parameter)
        {
            if (value is not bool bValue || parameter is not string formatString)
                return 1d;

            var values = formatString.Split('|');
            if (values.Length != 2)
                return 1d;

            return double.TryParse(bValue ? values[0] : values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
                ? result
                : 1d;
        }

        /// <inheritdoc/>
        protected override object? TryConvertBack(object? value, Type targetType, object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}
