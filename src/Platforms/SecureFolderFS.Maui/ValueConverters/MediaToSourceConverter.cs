using System.Globalization;
using CommunityToolkit.Maui.Views;
using SecureFolderFS.Maui.AppModels;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class MediaToSourceConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                VideoStreamServer videoServer => MediaSource.FromUri(videoServer.BaseAddress),
                _ => null
            };
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
