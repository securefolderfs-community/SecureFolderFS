using System;
using SecureFolderFS.Core.Storage;
using SecureFolderFS.Core.Tunnels;
using SecureFolderFS.Core.VaultDataStore;
using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

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
        IModifiableFolder VaultFolder { get; }

        string VolumeName { get; }

        VaultVersion VaultVersion { get; }

        ISecureFolderFSInstance SecureFolderFSInstance { get; }

        BaseVaultConfiguration BaseVaultConfiguration { get; }

        IFileTunnel FileTunnel { get; }

        IFolderTunnel FolderTunnel { get; }

        IVaultStorageReceiver VaultStorageReceiver { get; }
    }
}
