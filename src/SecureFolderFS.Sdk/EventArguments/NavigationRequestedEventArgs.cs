using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for navigation requests.
    /// </summary>
    public sealed class NavigationRequestedEventArgs(IResult result, IViewable origin) : EventArgs
    {
        /// <summary>
        /// Gets the result containing additional information about the navigation.
        /// </summary>
        public IResult Result { get; } = result;

        /// <summary>
        /// Gets the <see cref="IViewable"/> instance that invoked the navigation.
        /// </summary>
        public IViewable Origin { get; } = origin;
    }
}
