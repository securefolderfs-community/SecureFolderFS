using System;

namespace SecureFolderFS.UI.ValueConverters
{
    public abstract class BaseCountToBoolConverter : BaseConverter
    {
        /// <inheritdoc/>
        protected override object? TryConvert(object? value, Type targetType, object? parameter)
        {
            if (value is not int numValue)
                return false;

            if (parameter is string strParam && strParam.Equals("invert", StringComparison.OrdinalIgnoreCase))
                return !(numValue > 0);

            return numValue > 0;
        }

        /// <inheritdoc/>
        protected override object? TryConvertBack(object? value, Type targetType, object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}
