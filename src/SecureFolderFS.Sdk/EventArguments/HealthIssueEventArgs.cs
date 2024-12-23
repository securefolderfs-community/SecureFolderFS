using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for vault health issues.
    /// </summary>
    public sealed class HealthIssueEventArgs(IStorable storable, IWrapper<IResult> result) : EventArgs
    {
        /// <summary>
        /// Gets the affected <see cref="IStorable"/>.
        /// </summary>
        public IStorable Storable { get; } = storable;

        /// <summary>
        /// Gets the <see cref="IWrapper{T}"/> with <see cref="IResult"/> of the validation.
        /// </summary>
        public IWrapper<IResult> Result { get; } = result;
    }
}
