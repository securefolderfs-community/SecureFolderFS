using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.EventArguments;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for back navigation requests.
    /// </summary>
    public sealed class BackNavigationRequestedEventArgs(IViewable? origin) : NavigationRequestedEventArgs(origin);
}
