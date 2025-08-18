using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SecureFolderFS.Core.Models
{
    internal sealed class SecurityWrapper : IWrapper<Security>, IEnumerable<KeyValuePair<string, object>>, IDisposable
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

        public SecurityWrapper(KeyPair keyPair, VaultConfigurationDataModel configurationDataModel)
        {
            _keyPair = keyPair;
            _configDataModel = configurationDataModel;
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            yield return new(nameof(VirtualFileSystemOptions.RecycleBinSize), _configDataModel.RecycleBinSize);
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
