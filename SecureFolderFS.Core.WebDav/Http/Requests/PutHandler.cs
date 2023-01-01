using SecureFolderFS.Core.WebDav.Extensions;
using SecureFolderFS.Core.WebDav.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Extensions;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Http.Requests
{
    /// <inheritdoc cref="IRequestHandler"/>
    internal sealed class PutHandler : IRequestHandler
    {
        /// <inheritdoc/>
        public async Task ProcessRequestAsync(IHttpContext context, IStorageService storageService, CancellationToken cancellationToken = default)
        {
            if (context.Request.Url is null)
            {
                context.Response.SetStatus(HttpStatusCode.NotFound);
                return;
            }

            var parentUriPath = context.Request.Url.GetParentUriPath();
            var folder = await storageService.GetFolderFromPathAsync(parentUriPath.GetUriPath(), cancellationToken);
            if (folder is not IDavFolder davFolder)
            {
                context.Response.SetStatus(HttpStatusCode.InternalServerError);
                return;
            }

            var createdFileResult = await davFolder.CreateFileWithResultAsync(context.Request.Url.GetUriFileName(), CreationCollisionOption.ReplaceExisting, cancellationToken);
            if (createdFileResult.Successful)
            {
                var fileStreamResult = await createdFileResult.Value!.OpenStreamWithResultAsync(FileAccess.ReadWrite, cancellationToken);
                if (!fileStreamResult.Successful)
                {
                    context.Response.SetStatus(fileStreamResult);
                    return;
                }

                if (context.Request.InputStream is null)
                {
                    context.Response.SetStatus(HttpStatusCode.NoContent); // TODO: Is that error appropriate?
                    return;
                }

                var fileStream = fileStreamResult.Value!;
                await using (fileStream)
                {
                    // Make sure we can write to the file
                    if (!fileStream.CanWrite)
                    {
                        context.Response.SetStatus(HttpStatusCode.Conflict);
                        return;
                    }

                    // Copy contents
                    await context.Request.InputStream.CopyToAsync(fileStream, cancellationToken);

                    // TODO: Check for disk full exceptions

                    // Set status to OK
                    context.Response.SetStatus(HttpStatusCode.OK);
                }
            }
            else
                context.Response.SetStatus(createdFileResult);
        }
    }
}
