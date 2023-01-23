using SecureFolderFS.Core;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;
using SecureFolderFS.WinUI.AppModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultService"/>
    internal sealed class VaultService : IVaultService
    {
        private readonly Dictionary<string, IFileSystemInfoModel> _fileSystems;

        public VaultService()
        {
            _fileSystems = new()
            {
                { Core.Constants.FileSystemId.DOKAN_ID, new DokanyFileSystemDescriptor() },
                { Core.Constants.FileSystemId.WEBDAV_ID, new WebDavFileSystemDescriptor() }
            };
        }

        /// <inheritdoc/>
        public bool IsFileNameReserved(string? fileName)
        {
            return fileName is not null &&
                   (fileName.Equals(Core.Constants.VAULT_KEYSTORE_FILENAME, StringComparison.Ordinal) ||
                    fileName.Equals(Core.Constants.VAULT_CONFIGURATION_FILENAME, StringComparison.Ordinal) ||
                    fileName.Equals(Core.Constants.CONTENT_FOLDERNAME, StringComparison.Ordinal));
        }

        /// <inheritdoc/>
        public IAsyncValidator<IFolder> GetVaultValidator()
        {
            return VaultHelpers.NewVaultValidator(StreamSerializer.Instance);
        }

        /// <inheritdoc/>
        public IFileSystemInfoModel? GetFileSystemById(string id)
        {
            return _fileSystems.Values.FirstOrDefault(x => x.Id.Equals(id));
        }

        /// <inheritdoc/>
        public IEnumerable<IFileSystemInfoModel> GetFileSystems()
        {
            foreach (var item in _fileSystems.Values)
                yield return item;
        }

        /// <inheritdoc/>
        public IEnumerable<CipherInfoModel> GetContentCiphers()
        {
            yield return new CipherInfoModel("XChaCha20-Poly1305", Core.Constants.CipherId.XCHACHA20_POLY1305);
            yield return new CipherInfoModel("AES-GCM", Core.Constants.CipherId.AES_GCM);
        }

        /// <inheritdoc/>
        public IEnumerable<CipherInfoModel> GetFileNameCiphers()
        {
            yield return new CipherInfoModel("AES-SIV", Core.Constants.CipherId.AES_SIV);
            yield return new CipherInfoModel("None", Core.Constants.CipherId.NONE);
        }
    }
}
