using System;
using System.Collections.Generic;
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

            // Parse platform-specific parameters
            var platformParam = ParsePlatformParameter(formatStringParam);
            
            var isInverse = platformParam.Contains("inversemode:", StringComparison.OrdinalIgnoreCase);
            platformParam = isInverse ? platformParam.Replace("inversemode:", string.Empty, StringComparison.OrdinalIgnoreCase) : platformParam;

            if (!isInverse)
            {
                var rawPhrases = platformParam.Split(',');
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
                var modifiers = platformParam.Split('|');
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

        /// <summary>
        /// Parses platform-specific parameter and returns the appropriate value for the current platform
        /// </summary>
        private string ParsePlatformParameter(string parameter)
        {
            // Check if parameter contains platform-specific sections
            if (!parameter.Contains('{') || !parameter.Contains('}'))
                return parameter; // No platform-specific formatting

            var platformSections = new Dictionary<string, string>();
            var currentPlatform = GetCurrentPlatform();
            var position = 0;

            // Parse all platform sections
            while (position < parameter.Length)
            {
                var startBrace = parameter.IndexOf('{', position);
                if (startBrace == -1)
                    break;

                // Find platform name (text before the opening brace)
                var platformStart = position;
                while (platformStart < startBrace && char.IsWhiteSpace(parameter[platformStart]))
                    platformStart++;

                var platformName = parameter.Substring(platformStart, startBrace - platformStart);

                // Find matching closing brace
                var braceCount = 1;
                var endBrace = startBrace + 1;
                while (endBrace < parameter.Length && braceCount > 0)
                {
                    if (parameter[endBrace] == '{')
                        braceCount++;
                    else if (parameter[endBrace] == '}')
                        braceCount--;
                    endBrace++;
                }

                if (braceCount == 0)
                {
                    var content = parameter.Substring(startBrace + 1, endBrace - startBrace - 2);
                    platformSections[platformName] = content;
                    position = endBrace;
                }
                else
                {
                    break; // Malformed parameter
                }
            }

            // Return the appropriate platform-specific value
            if (platformSections.TryGetValue(currentPlatform, out var platformValue))
                return platformValue;

            if (platformSections.TryGetValue("Default", out var defaultValue))
                return defaultValue;

            // If no platform sections found or no match, return the original parameter
            return parameter;
        }
        
        /// <summary>
        /// Gets the current platform identifier (e.g., "Windows", "MacOS", "Linux")
        /// </summary>
        private static string GetCurrentPlatform()
        {
            if (OperatingSystem.IsMacOS())
                return "MacOS";
            
            if (OperatingSystem.IsWindows())
                return "Windows";
            
            if (OperatingSystem.IsLinux())
                return "Linux";
            
            return "Unknown";
        }
    }
}
