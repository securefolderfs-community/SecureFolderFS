using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.EventArguments;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for vault unlock navigation requests.
    /// </summary>
    public sealed class UnlockNavigationRequestedEventArgs(UnlockedVaultViewModel unlockedVaultViewModel, IViewable? origin) : NavigationRequestedEventArgs(origin)
    {
        /// <summary>
        /// Gets the <see cref="UnlockedVaultViewModel"/> of the unlocked vault.
        /// </summary>
        public UnlockedVaultViewModel UnlockedVaultViewModel { get; } = unlockedVaultViewModel;
    }
}
