using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for vault health issues.
    /// </summary>
    public sealed class HealthIssueEventArgs(IResult result, IStorable? storable) : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="IResult"/> of the validation.
        /// </summary>
        public IResult Result { get; } = result;

        /// <summary>
        /// Gets the affected <see cref="IStorable"/>, if any.
        /// </summary>
        public IStorable? Storable { get; } = storable;
    }
}
