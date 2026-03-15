using System;

namespace SecureFolderFS.UI.ValueConverters
{
    public abstract class BaseBoolOpacityConverter : BaseConverter
    {
        /// <inheritdoc/>
        protected override object? TryConvert(object? value, Type targetType, object? parameter)
        {
            if (value is not bool bValue)
                return 0d;

            if (parameter is string strParam && strParam.Equals("invert", StringComparison.OrdinalIgnoreCase))
                return bValue ? 0d : 1d;

            return bValue ? 1d : 0d;
        }

        /// <inheritdoc/>
        protected override object? TryConvertBack(object? value, Type targetType, object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}
