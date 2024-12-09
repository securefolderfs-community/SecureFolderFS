using System;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.UI.ValueConverters
{
    public abstract class BaseSeverityHealthIconConverter : BaseConverter
    {
        protected abstract object? SuccessIcon { get; }

        protected abstract object? WarningIcon { get; }

        protected abstract object? CriticalIcon { get; }

        /// <inheritdoc/>
        protected override object? TryConvert(object? value, Type targetType, object? parameter)
        {
            if (value is not SeverityType severity)
                return null;

            return severity switch
            {
                SeverityType.Warning => WarningIcon,
                SeverityType.Critical => CriticalIcon,
                _ => SuccessIcon
            };
        }

        /// <inheritdoc/>
        protected override object? TryConvertBack(object? value, Type targetType, object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}
