using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Storage.Extensions
{
    public static class NativeExtensions
    {
        public static async Task<Stream> OpenStreamAsync(this IFile file, FileAccess accessMode, FileShare shareMode, CancellationToken cancellationToken = default)
        {
            if (file is SystemFile systemFile)
                return systemFile.Info.Open(new FileStreamOptions()
                {
                    Access = accessMode, Share = shareMode, Options = FileOptions.Asynchronous
                });

            return await file.OpenStreamAsync(accessMode, cancellationToken);
        }
    }
}
