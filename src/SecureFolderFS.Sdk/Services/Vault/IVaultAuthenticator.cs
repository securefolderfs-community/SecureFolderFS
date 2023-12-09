using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Storage;
using System.Collections.Generic;
using System.Threading;

namespace SecureFolderFS.Sdk.Services.Vault
{
    public interface IVaultAuthenticator
    {
        IAsyncEnumerable<AuthenticationModel> GetAuthenticationAsync(IFolder vaultFolder, CancellationToken cancellationToken = default);

        IAsyncEnumerable<AuthenticationModel> GetAvailableAuthenticationsAsync(CancellationToken cancellationToken = default);
    }
}
