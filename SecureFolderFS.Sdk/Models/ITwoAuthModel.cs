using System;

namespace SecureFolderFS.Sdk.Models
{
    public interface ITwoAuthModel : IDisposable
    {
        /// <summary>
        /// Gets associated <see cref="IVaultModel"/> with this model.
        /// </summary>
        IVaultModel VaultModel { get; }
    }
}
