using System;
using System.Collections.Generic;
using System.Linq;

namespace SecureFolderFS.UI.ValueConverters
{
    public abstract class BaseGenericEnumConverter : BaseConverter
    {
        /// <inheritdoc/>
        protected override object? TryConvert(object? value, Type targetType, object? parameter)
        {
            return ConvertInternal(value, targetType, parameter,
                s => s.Split(',').ToDictionary(k => System.Convert.ToInt64(k.Split('-')[0]), v => System.Convert.ToInt64(v.Split('-')[1])));
        }

        /// <inheritdoc/>
        protected override object? TryConvertBack(object? value, Type targetType, object? parameter)
        {
            return ConvertInternal(value, targetType, parameter,
                s => s.Split(',').ToDictionary(k => System.Convert.ToInt64(k.Split('-')[0]), v => System.Convert.ToInt64(v.Split('-')[1])));
        }

        private object ConvertInternal(object? value, Type targetType, object? parameter, Func<string, Dictionary<long, long>> enumConversion)
        {
            var enumValue = Convert.ToInt64(value);
            if (parameter is string strParam)
            {
                // enumValue-convertedValue: 0-1,1-2
                var enumConversionValues = enumConversion(strParam);
                if (enumConversionValues.TryGetValue(enumValue, out var convertedValue))
                    enumValue = convertedValue;

                // else.. use value from the cast above
            }

            try
            {
                if (Enum.GetName(targetType, enumValue) is string enumName)
                    return Enum.Parse(targetType, enumName);
            }
            catch (Exception) { }

            try
            {
                return System.Convert.ChangeType(enumValue, targetType);
            }
            catch (Exception) { }

            return enumValue;
        }
    }
}
