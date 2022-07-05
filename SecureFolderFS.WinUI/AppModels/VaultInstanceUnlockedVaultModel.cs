﻿using System.Threading.Tasks;
using SecureFolderFS.Core.Instance;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.WinUI.AppModels
{
    internal sealed class VaultInstanceUnlockedVaultModel : IUnlockedVaultModel
    {
        private readonly IVaultInstance _vaultInstance;

        /// <inheritdoc/>
        public IFolder RootFolder { get; }

        /// <inheritdoc/>
        public IFileSystemService VaultFileSystem { get; }

        public VaultInstanceUnlockedVaultModel(IVaultInstance vaultInstance, IFolder rootFolder)
        {
            _vaultInstance = vaultInstance;
            RootFolder = rootFolder;
        }

        /// <inheritdoc/>
        public Task LockAsync()
        {
            _vaultInstance.Dispose();
            return Task.CompletedTask;
        }
    }
}