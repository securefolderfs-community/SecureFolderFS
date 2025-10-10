using System;
using System.Diagnostics.CodeAnalysis;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Shared.Extensions
{
    public static class GenericExtensions
    {
        [return: NotNullIfNotNull(nameof(defaultValue))]
        public static TOut? TryCast<TOut>(this object? value, Func<TOut>? defaultValue = null)
        {
            if (value is TOut outValue)
                return outValue;

            return defaultValue is not null ? defaultValue.Invoke() : default;
        }

        [return: NotNullIfNotNull(nameof(defaultValue))]
        public static TOut? TryInnerCast<TOut, TSelf>(this TSelf? value, Func<TOut>? defaultValue = null)
        {
            if (value is IWrapper<TSelf> { Inner: TOut } && value.TryCast(defaultValue) is var outValue)
                return outValue;

            return defaultValue is not null ? defaultValue.Invoke() : default;
        }
    }
}
