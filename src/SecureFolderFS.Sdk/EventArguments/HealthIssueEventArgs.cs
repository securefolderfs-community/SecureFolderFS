using OwlCore.Storage;
using SecureFolderFS.Sdk.Results;
using System;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for vault health issues.
    /// </summary>
    public sealed class HealthIssueEventArgs(IStorable storable, IHealthResult healthResult) : EventArgs
    {
        /// <summary>
        /// Gets the affected <see cref="IStorable"/>.
        /// </summary>
        public IStorable Storable { get; } = storable;

        /// <summary>
        /// Gets the <see cref="IHealthResult"/> of the validation.
        /// </summary>
        public IHealthResult Result { get; } = healthResult;
    }
}
