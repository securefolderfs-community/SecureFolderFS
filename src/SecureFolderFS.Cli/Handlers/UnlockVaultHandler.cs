using SecureFolderFS.Cli.Helpers;
using SecureFolderFS.Cli.Options;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Cli.Handlers
{
    public sealed class UnlockVaultHandler : SingletonBase<UnlockVaultHandler>, IHandler<UnlockVaultOptions>
    {
        public async Task HandleAsync(UnlockVaultOptions options, CancellationToken cancellationToken = default)
        {
            ValidationHelper.ValidateOptions(options);
        }
    }
}