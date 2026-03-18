using System.Globalization;
using SecureFolderFS.Maui.AppModels;

namespace SecureFolderFS.Maui.ValueConverters
{
    internal sealed class FileServerWebViewSourceConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not PdfStreamServer server)
                return null;

            var fileUrl = $"{server.BaseAddress}/app_file";
#if ANDROID
            var viewerUrl = $"{server.BaseAddress}/pdfjs53/web/viewer.html?file={System.Net.WebUtility.UrlEncode(fileUrl)}";
            return new UrlWebViewSource()
            {
                Url = viewerUrl
            };
#elif IOS
            return new UrlWebViewSource()
            {
                Url = fileUrl
            };
#endif
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
