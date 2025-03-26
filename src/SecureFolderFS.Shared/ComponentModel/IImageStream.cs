using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.ComponentModel
{
    public interface IImageStream : IImage
    {
        Task CopyToAsync(Stream destination, CancellationToken cancellationToken = default);
    }
}
