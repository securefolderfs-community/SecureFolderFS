using System;

namespace SecureFolderFS.UI.ValueConverters
{
    /// <summary>
    /// Provides a base class for value converters.
    /// </summary>
    public abstract class BaseConverter
    {
        /// <summary>
        /// Attempts to convert a value to the specified target type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type to convert the value to.</param>
        /// <param name="parameter">An optional parameter to use during the conversion.</param>
        /// <returns>The converted value, or null if the conversion failed.</returns>
        protected abstract object? TryConvert(object? value, Type targetType, object? parameter);

        /// <summary>
        /// Attempts to convert a value back to the specified target type.
        /// </summary>
        /// <param name="value">The value to convert back.</param>
        /// <param name="targetType">The type to convert the value back to.</param>
        /// <param name="parameter">An optional parameter to use during the conversion.</param>
        /// <returns>The converted value, or null if the conversion failed.</returns>
        protected abstract object? TryConvertBack(object? value, Type targetType, object? parameter);
    }
}
