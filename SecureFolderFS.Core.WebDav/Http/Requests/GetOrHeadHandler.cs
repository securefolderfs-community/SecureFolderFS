using SecureFolderFS.Core.WebDav.Extensions;
using SecureFolderFS.Core.WebDav.Storage;
using SecureFolderFS.Core.WebDav.Storage.StorageProperties;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.StorageProperties;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Http.Requests
{
    /// <inheritdoc cref="IRequestHandler"/>
    internal sealed class GetOrHeadHandler : IRequestHandler
    {
        /// <inheritdoc/>
        public async Task ProcessRequestAsync(IHttpContext context, IStorageService storageService, CancellationToken cancellationToken = default)
        {
            if (context.Request.Url is null)
            {
                context.Response.SetStatus(HttpStatusCode.NotFound);
                return;
            }

            var storable = await storageService.GetStorableFromPathAsync(context.Request.Url.GetUriPath(), cancellationToken);
            if (storable is IDavStorable davStorable)
            {
                // Set properties for the response
                await SetPropertiesAsync(context.Response, davStorable.Properties, cancellationToken);
            }

            // Read content
            if (storable is IFile file)
            {
                try
                {
                    await using var fileStream = await file.OpenStreamAsync(FileAccess.Read, cancellationToken);
                    if (fileStream.CanSeek)
                    {
                        context.Response.SetHeaderValue(Constants.Headers.ACCEPT_RANGES, "bytes");

                        // TODO: Range

                        context.Response.SetHeaderValue(Constants.Headers.CONTENT_LENGTH, fileStream.Length.ToString());
                    }

                    // Copy contents
                    if (context.Request.HttpMethod != "HEAD")
                        await fileStream.CopyToAsync(context.Response.OutputStream, cancellationToken);
                }
                catch (Exception ex)
                {
                    _ = ex;
                    Debugger.Break();
                    
                    throw;
                }
            }
            else
                context.Response.SetStatus(HttpStatusCode.NoContent);
        }

        private static async Task SetPropertiesAsync(IHttpResponse response, IBasicProperties properties, CancellationToken cancellationToken)
        {
            var dateModified = await properties.GetDateModifiedAsync(cancellationToken);
            response.SetHeaderValue(Constants.Headers.LAST_MODIFIED, dateModified.Value.ToString("R")); // R - RFC1123 compliant

            if (properties is IDavProperties davProperties)
            {
                var etag = await davProperties.GetEtagAsync(true, cancellationToken);
                if (etag is not null)
                    response.SetHeaderValue(Constants.Headers.E_TAG, etag.Value);

                var contentType = await davProperties.GetContentTypeAsync(true, cancellationToken);
                if (contentType is not null)
                    response.SetHeaderValue(Constants.Headers.CONTENT_TYPE, contentType.Value);

                var contentLanguage = await davProperties.GetContentLanguageAsync(true, cancellationToken);
                if (contentLanguage is not null)
                    response.SetHeaderValue(Constants.Headers.CONTENT_LANGUAGE, contentLanguage.Value);
            }
        }
    }
}
