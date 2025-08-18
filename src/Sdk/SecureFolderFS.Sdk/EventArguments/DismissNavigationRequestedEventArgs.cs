using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.EventArguments;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for close navigation requests.
    /// </summary>
    public sealed class DismissNavigationRequestedEventArgs(IViewable? origin) : NavigationRequestedEventArgs(origin);
}
