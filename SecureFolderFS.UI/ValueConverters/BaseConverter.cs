using System;

namespace SecureFolderFS.UI.ValueConverters
{
    // TODO: Needs docs
    public abstract class BaseConverter
    {
        protected abstract object? TryConvert(object? value, Type targetType, object? parameter);

        protected abstract object? TryConvertBack(object? value, Type targetType, object? parameter);
    }
}
