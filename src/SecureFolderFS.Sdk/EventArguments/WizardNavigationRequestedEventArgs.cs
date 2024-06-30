using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.EventArguments;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for wizard navigation requests.
    /// </summary>
    public sealed class WizardNavigationRequestedEventArgs(IResult result, IViewable origin)
        : NavigationRequestedEventArgs(origin)
    {
        /// <summary>
        /// Gets the result containing additional information about the navigation.
        /// </summary>
        public IResult Result { get; } = result;
    }
}
