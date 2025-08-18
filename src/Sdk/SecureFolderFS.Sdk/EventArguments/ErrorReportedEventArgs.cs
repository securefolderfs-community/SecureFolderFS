using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for reporting <see cref="IResult"/> errors.
    /// </summary>
    public sealed class ErrorReportedEventArgs(IResult result) : EventArgs
    {
        /// <summary>
        /// Gets the reported error result.
        /// </summary>
        public IResult Result { get; } = result;
    }
}
