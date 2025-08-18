using System.Globalization;
using System.Net;
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
            var viewerUrl = $"{server.BaseAddress}/pdfjs53/web/viewer.html?file={WebUtility.UrlEncode(fileUrl)}";
            return new UrlWebViewSource()
            {
                Url = viewerUrl
            };
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
