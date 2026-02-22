using System;

namespace SecureFolderFS.UI.ValueConverters
{
    public abstract class BaseSubstringConverter : BaseConverter
    {
        /// <inheritdoc/>
        protected override object? TryConvert(object? value, Type targetType, object? parameter)
        {
            if (parameter is not string strParam)
                return null;
            
            var text = value as string;
            if (string.IsNullOrEmpty(text))
                return text;

            try
            {
                var args = strParam.Split(',');
                var startIndex = int.Parse(args[0]);
                var length = int.Parse(args[1]);

                // Basic safety check for string length
                if (startIndex >= text.Length)
                    return string.Empty;
                
                if (startIndex + length > text.Length)
                    length = text.Length - startIndex;

                return text.Substring(startIndex, length);
            }
            catch (Exception)
            {
                return text;
            }
        }

        /// <inheritdoc/>
        protected override object? TryConvertBack(object? value, Type targetType, object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}
