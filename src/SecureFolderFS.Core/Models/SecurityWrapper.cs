using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Core.Models
{
    internal sealed class SecurityWrapper : IWrapper<Security>, IDisposable
    {
        private readonly KeyPair _keyPair;
        private readonly VaultConfigurationDataModel _configurationDataModel;
        private Security? _security;

        /// <inheritdoc/>
        public Security Inner => _security ??= Security.CreateNew(
                _keyPair,
                contentCipherId: _configurationDataModel.ContentCipherId,
                fileNameCipherId: _configurationDataModel.FileNameCipherId,
                fileNameEncodingId: _configurationDataModel.FileNameEncodingId);

        public SecurityWrapper(KeyPair keyPair, VaultConfigurationDataModel configurationDataModel)
        {
            _keyPair = keyPair;
            _configurationDataModel = configurationDataModel;
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
