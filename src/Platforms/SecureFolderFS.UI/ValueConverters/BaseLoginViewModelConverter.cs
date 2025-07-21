using System;

namespace SecureFolderFS.UI.ValueConverters
{
    public abstract class BaseLoginViewModelConverter : BaseConverter
    {
        /// <inheritdoc/>
        protected override object? TryConvert(object? value, Type targetType, object? parameter)
        {
            if (parameter is not string strParam)
                return false;

            var split = strParam.Split('|');
            var invert = split[1].Contains("invert", StringComparison.OrdinalIgnoreCase);
            var result = false;

            foreach (var item in split[0].Split(','))
            {
                result = value?.GetType().Name.Equals(item, StringComparison.OrdinalIgnoreCase) ?? false;
                if (result)
                    break;
            }

            return invert ? !result : result;
        }

        /// <inheritdoc/>
        protected sealed override object? TryConvertBack(object? value, Type targetType, object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}
