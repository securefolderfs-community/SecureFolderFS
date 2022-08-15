using System;
using SecureFolderFS.Core.VaultDataStore;
using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.Core.Instance
{
    /// <summary>
    /// Provides module for managing the vault instance.
    /// <br/>
    /// <br/>
    /// This API is exposed.
    /// </summary>
    public interface IVaultInstance : IDisposable
    {
        IFolder VaultFolder { get; }

        string VolumeName { get; }

        VaultVersion VaultVersion { get; }

        ISecureFolderFSInstance SecureFolderFSInstance { get; }

        BaseVaultConfiguration BaseVaultConfiguration { get; }
    }
}
