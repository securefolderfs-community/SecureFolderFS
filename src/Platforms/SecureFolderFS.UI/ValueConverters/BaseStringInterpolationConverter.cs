using System;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.UI.ValueConverters
{
    public abstract class BaseStringInterpolationConverter : BaseConverter
    {
        /// <inheritdoc/>
        protected override object? TryConvert(object? value, Type targetType, object? parameter)
        {
            if (value is not string strValue)
                return string.Empty;

            if (parameter is not string formatStringParam)
                return strValue;

            var isInverse = formatStringParam.Contains("inversemode:", StringComparison.OrdinalIgnoreCase);
            formatStringParam = isInverse ? formatStringParam.Replace("inversemode:", string.Empty, StringComparison.OrdinalIgnoreCase) : formatStringParam;

            if (!isInverse)
            {
                var rawPhrases = formatStringParam.Split(',');
                if (rawPhrases.IsEmpty())
                    return strValue.ToLocalized();

                var phrases = new object[rawPhrases.Length];
                for (var i = 0; i < rawPhrases.Length; i++)
                {
                    var modifiers = rawPhrases[i].Split('|');
                    var format = modifiers[0];
                    var text = modifiers[1];

                    phrases[i] = format.Equals("localize", StringComparison.OrdinalIgnoreCase)
                        ? text.ToLocalized()
                        : text;
                }

                return SafetyHelpers.NoFailureResult(() => string.Format(strValue, phrases));
            }
            else
            {
                var modifiers = formatStringParam.Split('|');
                var format = modifiers[0];
                var text = modifiers[1];

                return format.Equals("localize", StringComparison.OrdinalIgnoreCase)
                    ? text.ToLocalized(strValue)
                    : SafetyHelpers.NoFailureResult(() => string.Format(text, strValue));
            }
        }

        /// <inheritdoc/>
        protected override object? TryConvertBack(object? value, Type targetType, object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}
