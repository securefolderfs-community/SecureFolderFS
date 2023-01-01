using SecureFolderFS.Core.WebDav.Extensions;
using SecureFolderFS.Core.WebDav.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Extensions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Http.Requests
{
    /// <inheritdoc cref="IRequestHandler"/>
    internal sealed class MkcolHandler : IRequestHandler
    {
        /// <inheritdoc/>
        public async Task ProcessRequestAsync(IHttpContext context, IStorageService storageService, CancellationToken cancellationToken = default)
        {
            if (context.Request.Url is null)
            {
                context.Response.SetStatus(HttpStatusCode.NotFound);
                return;
            }

            // Get parent folder
            var parentUriPath = context.Request.Url.GetParentUriPath();
            var folder = await storageService.GetFolderFromPathAsync(parentUriPath.GetUriPath(), cancellationToken);
            if (folder is not IDavFolder davFolder)
            {
                context.Response.SetStatus(HttpStatusCode.InternalServerError);
                return;
            }

            // Create new folder
            var createdFolderResult = await davFolder.CreateFolderWithResultAsync(context.Request.Url.GetUriFileName(), CreationCollisionOption.FailIfExists, cancellationToken);
            context.Response.SetStatus(createdFolderResult);
        }
    }
}
