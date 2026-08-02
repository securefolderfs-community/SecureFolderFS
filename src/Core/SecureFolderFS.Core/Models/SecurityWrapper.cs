using System;
using System.Collections;
using System.Collections.Generic;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.Models
{
    internal sealed class SecurityWrapper : IWrapper<Security>, IWrapper<KeyPair>, IWrapper<VaultConfigurationDataModel>, IEnumerable<KeyValuePair<string, object>>, IDisposable
    {
        private readonly KeyPair _keyPair;
        private readonly VaultConfigurationDataModel _configDataModel;
        private Security? _security;

        /// <inheritdoc/>
        public Security Inner => _security ??= Security.CreateNew(
                _keyPair,
                contentCipherId: _configDataModel.ContentCipherId,
                fileNameCipherId: _configDataModel.FileNameCipherId,
                fileNameEncodingId: _configDataModel.FileNameEncodingId);

        /// <inheritdoc/>
        KeyPair IWrapper<KeyPair>.Inner => _keyPair;

        /// <summary>
        /// Gets the vault configuration whose MAC was verified during unlock.
        /// </summary>
        /// <remarks>
        /// Routines that rewrite the configuration must derive it from this model rather than from a
        /// fresh unvalidated read of the vault directory, otherwise a configuration an attacker edited
        /// on disk would be re-signed with the vault's genuine MAC key.
        /// </remarks>
        VaultConfigurationDataModel IWrapper<VaultConfigurationDataModel>.Inner => _configDataModel;

        public SecurityWrapper(KeyPair keyPair, VaultConfigurationDataModel configDataModel)
        {
            _keyPair = keyPair;
            _configDataModel = configDataModel;
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            yield return new(nameof(VirtualFileSystemOptions.RecycleBinSize), _configDataModel.RecycleBinSize);
            yield return new (nameof(VirtualFileSystemOptions.ShorteningThreshold), _configDataModel.ShorteningThreshold);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _keyPair.ToString();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Inner.Dispose();
            _security?.Dispose();
        }
    }
}
