using System.Collections.Generic;
using System.Threading;
using OwlCore.Storage;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Cli;

internal sealed class CliVaultCredentialsService : BaseVaultCredentialsService
{
    public override async IAsyncEnumerable<AuthenticationViewModel> GetLoginAsync(IFolder vaultFolder, CancellationToken cancellationToken = default)
    {
        _ = vaultFolder;
        await Task.CompletedTask;
        yield break;
    }

    public override async IAsyncEnumerable<AuthenticationViewModel> GetCreationAsync(IFolder vaultFolder, string vaultId,
        CancellationToken cancellationToken = default)
    {
        _ = vaultFolder;
        _ = vaultId;
        await Task.CompletedTask;
        yield break;
    }
}

