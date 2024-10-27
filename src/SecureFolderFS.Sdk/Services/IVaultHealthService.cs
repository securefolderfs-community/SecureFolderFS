using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    public interface IVaultHealthService
    {
        Task<IResult> ScanFileAsync(IFile file, CancellationToken cancellationToken = default);

        Task<IResult> ScanFolderAsync(IFolder folder, CancellationToken cancellationToken = default);
    }
}
