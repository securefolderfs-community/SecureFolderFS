using SecureFolderFS.Sdk.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Http
{
    // TODO: Needs docs
    internal interface IRequestHandler
    {
        // TODO: Maybe pass IStorageService instead of IDavStorable so the handler could access any file/folder rather than only one.
        Task ProcessRequestAsync(IHttpContext context, IStorageService storageService, CancellationToken cancellationToken = default);
    }
}
