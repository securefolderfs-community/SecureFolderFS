using System;

namespace SecureFolderFS.Shared.Extensions
{
    public static class GenericExtensions
    {
        public static TOut? TryCast<TOut>(this object? value, Func<TOut?>? defaultValue = null)
        {
            if (value is TOut outValue)
                return outValue;

            return defaultValue is not null ? defaultValue.Invoke() : default;
        }
    }
}
