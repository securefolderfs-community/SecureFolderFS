using SecureFolderFS.Core.WebDav.Http;
using SecureFolderFS.Shared.Utils;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SecureFolderFS.Core.WebDav.Extensions
{
    internal static class HttpExtensions
    {
        public static void SetStatus(this IHttpResponse response, HttpStatusCode statusCode, string? description = null)
        {
            response.StatusCode = (int)statusCode;
            response.StatusDescription = description ?? statusCode.ToString();
        }

        public static void SetStatus(this IHttpResponse response, IResult result)
        {
            throw new NotImplementedException();
        }

        public static int GetEnumerationDepth(this IHttpRequest request)
        {
            var depthHeader = request.GetHeaderValue(Constants.Headers.DEPTH);
            if (depthHeader is null or "infinity")
                return int.MaxValue;

            if (!int.TryParse(depthHeader, out var depth))
                return int.MaxValue;

            return depth;
        }

        public static async Task SendResponseAsync(this IHttpResponse response, HttpStatusCode statusCode,
            XDocument xDocument, CancellationToken cancellationToken = default)
        {
            _ = xDocument.Root ?? throw new ArgumentException("The XDoc does not contain a root.");

            response.SetStatus(statusCode);

            await using var ms = new MemoryStream();
            await using (var xmlWriter = XmlWriter.Create(ms, new XmlWriterSettings()
            {
                OmitXmlDeclaration = false,
#if DEBUG
                Indent = true,
#else
                Indent = false,
#endif
                Encoding = Encoding.UTF8,
            }))
            {
                // Add the namespaces (Win7 WebDAV client requires them like this)
                xDocument.Root.SetAttributeValue(XNamespace.Xmlns + Constants.WebDavNamespaces.DAV_PREFIX_NAMESPACE, (XNamespace)Constants.WebDavNamespaces.DAV_NAMESPACE);
                xDocument.Root.SetAttributeValue(XNamespace.Xmlns + Constants.WebDavNamespaces.DAV_WIN32_PREFIX_NAMESPACE, (XNamespace)Constants.WebDavNamespaces.DAV_WIN32_NAMESPACE);

                // Write the XML document to the stream
                await xDocument.WriteToAsync(xmlWriter, cancellationToken);
            }

            // Flush
            await ms.FlushAsync(cancellationToken);

            // Set content type/length
            response.SetHeaderValue("Content-Type", "text/xml; charset=\"utf-8\"");
            response.SetHeaderValue("Content-Length", ms.Position.ToString(CultureInfo.InvariantCulture));

            // Reset stream and write the stream to the result
            ms.Seek(0, SeekOrigin.Begin);
            await ms.CopyToAsync(response.OutputStream, cancellationToken);
        }

        public static async Task<XDocument?> GetDocumentFromRequestAsync(this IHttpRequest request, CancellationToken cancellationToken)
        {
            if (request.InputStream is null)
                return null;

            var contentLengthString = request.GetHeaderValue("Content-Length");
            if (contentLengthString != null)
            {
                if (!int.TryParse(contentLengthString, out var contentLength) || contentLength == 0)
                    return null;
            }

            return await XDocument.LoadAsync(request.InputStream, LoadOptions.None, cancellationToken);
        }
    }
}
