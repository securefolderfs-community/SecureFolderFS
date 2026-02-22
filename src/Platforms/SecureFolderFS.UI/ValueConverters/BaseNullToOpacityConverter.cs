using System;

namespace SecureFolderFS.UI.ValueConverters
{
    public abstract class BaseNullToOpacityConverter : BaseConverter
    {
        /// <inheritdoc/>
        protected override object? TryConvert(object? value, Type targetType, object? parameter)
        {
            var invert = parameter?.ToString()?.ToLower() == "invert";
            if (value is null)
                return invert ? 1d : 0d;
            
            return invert ? 0d : 1d;
        }
        
        /// <inheritdoc/>
        protected override object? TryConvertBack(object? value, Type targetType, object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}
