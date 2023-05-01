using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    // TODO: needs docs
    public interface IPasswordChangeService : IDisposable
    {
        Task<IResult> SetVaultFolderAsync(IFolder vaultFolder, CancellationToken cancellationToken);

        Task<IResult> SetKeystoreAsync(IKeystoreModel keystoreModel, CancellationToken cancellationToken);

        Task<IResult> ChangePasswordAsync(IPassword existingPassword, IPassword newPassword, CancellationToken cancellationToken);
    }
}
