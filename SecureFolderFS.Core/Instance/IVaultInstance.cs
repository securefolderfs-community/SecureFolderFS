using System;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Storage;
using SecureFolderFS.Core.Tunnels;
using SecureFolderFS.Core.VaultDataStore;

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
        VaultPath VaultPath { get; }

        string VolumeName { get; }

        VaultVersion VaultVersion { get; }

        ISecureFolderFSInstance SecureFolderFSInstance { get; }

        IFileTunnel FileTunnel { get; }

        IFolderTunnel FolderTunnel { get; }

        IVaultStorageReceiver VaultStorageReceiver { get; }
    }
}
