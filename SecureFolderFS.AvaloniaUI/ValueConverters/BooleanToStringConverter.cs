using System;
using System.Globalization;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.AvaloniaUI.ValueConverters
{
    internal sealed class BooleanToStringConverter : IValueConverter
    {
        private static ILocalizationService LocalizationService { get; } = Ioc.Default.GetRequiredService<ILocalizationService>();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not bool bValue)
                return string.Empty;

            if (parameter is not string formatString)
                return bValue.ToString();

            // Example:
            // If false, localize using "StringRes" resource. If true, don't localize, just use "Some string"
            // false:LOCALIZE|StringRes:true:STANDARD|Some string

            var valueSplit = formatString.Split(':');
            var splitOption = bValue ? valueSplit[3].Split('|') : valueSplit[1].Split('|');

            return splitOption[0] == "LOCALIZE" ? LocalizationService.LocalizeString(splitOption[1]) : splitOption[1];
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
