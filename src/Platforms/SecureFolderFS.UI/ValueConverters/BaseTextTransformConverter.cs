using System;

namespace SecureFolderFS.UI.ValueConverters
{
    public abstract class BaseTextTransformConverter : BaseConverter
    {
        /// <inheritdoc/>
        protected override object? TryConvert(object? value, Type targetType, object? parameter)
        {
            if (value is not string strValue)
                return null;
            
            if (parameter is not string strParam)
                return strValue;

            return strParam switch
            {
                "uppercase" => strValue.ToUpper(),
                "lowercase" => strValue.ToLower(),
                "firstuppercase" => string.Concat(strValue.Substring(0, 1).ToUpper(), strValue.AsSpan(1)),
                _ => strValue
            };
        }

        /// <inheritdoc/>
        protected override object? TryConvertBack(object? value, Type targetType, object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}
