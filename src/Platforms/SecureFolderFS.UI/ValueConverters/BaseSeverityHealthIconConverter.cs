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
            if (value is not Severity severity)
                return null;

            return severity switch
            {
                Severity.Warning => WarningIcon,
                Severity.Critical => CriticalIcon,
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
