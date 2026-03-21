using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Authentication
{
    /// <summary>
    /// Allows authentication view models to inject additional vault metadata during creation.
    /// </summary>
    public interface IVaultOptionsProvider
    {
        VaultOptions AmendVaultOptions(VaultOptions options);
    }
}

