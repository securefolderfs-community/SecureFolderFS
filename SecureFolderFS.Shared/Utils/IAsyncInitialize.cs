using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Utils
{
    public interface IAsyncInitialize
    {
        Task InitAsync(CancellationToken cancellationToken = default);
    }
}
