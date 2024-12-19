using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.Contexts
{
    /// <summary>
    /// A context for interacting with <see cref="Sdk.ViewModels.VaultViewModel"/>.
    /// </summary>
    public interface IVaultViewContext : IViewable
    {
        /// <summary>
        /// Gets the <see cref="Sdk.ViewModels.VaultViewModel"/> associated with this context.
        /// </summary>
        VaultViewModel VaultViewModel { get; }
    }
}
