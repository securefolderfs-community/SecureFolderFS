using System;

namespace SecureFolderFS.UI.ValueConverters
{
    public abstract class BaseNullToBoolConverter : BaseConverter
    {
        /// <inheritdoc/>
        protected override object? TryConvert(object? value, Type targetType, object? parameter)
        {
            if (parameter is string strParam && strParam.ToLower() == "invert")
            {
                if (value is string str1)
                    return string.IsNullOrEmpty(str1);

                return value is null;
            }

            if (value is string str)
                return !string.IsNullOrEmpty(str);

            return value is not null;
        }

        /// <inheritdoc/>
        protected sealed override object? TryConvertBack(object? value, Type targetType, object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}
