using System;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Helpers;

namespace SecureFolderFS.UI.ValueConverters
{
    public abstract class BasePasswordToStrengthConverter : BaseConverter
    {
        /// <inheritdoc/>
        protected override object? TryConvert(object? value, Type targetType, object? parameter)
        {
            if (value is not string password)
                return 0d;

            var normalization = 1d;
            if (parameter is string strParam && double.TryParse(strParam, out var dParam))
                normalization = dParam;

            return ValidationHelpers.ValidatePassword(password) switch
            {
                PasswordStrength.VeryWeak => 10d,
                PasswordStrength.Weak => 35d,
                PasswordStrength.Medium => 60d,
                PasswordStrength.Strong => 85d,
                PasswordStrength.VeryStrong => 100d,
                _ => 0d
            } * normalization;
        }

        /// <inheritdoc/>
        protected override object? TryConvertBack(object? value, Type targetType, object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}
