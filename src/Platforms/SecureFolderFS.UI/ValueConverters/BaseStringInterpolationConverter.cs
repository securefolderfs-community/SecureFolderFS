using System;
using SecureFolderFS.Sdk.Extensions;
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
            
            if (parameter is not string formatString)
                return strValue;

            var rawPhrases = formatString.Split(',');
            var phrases = new object[rawPhrases.Length];
            foreach (var phrase in rawPhrases)
            {
                var modifiers = phrase.Split('|');
                var format = modifiers[0];
                var text = modifiers[1];

                phrases[0] = format.Equals("localize", StringComparison.OrdinalIgnoreCase)
                    ? text.ToLocalized()
                    : text;
            }

            return SafetyHelpers.NoFailureResult(() => string.Format(strValue, phrases));
        }

        /// <inheritdoc/>
        protected override object? TryConvertBack(object? value, Type targetType, object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}
