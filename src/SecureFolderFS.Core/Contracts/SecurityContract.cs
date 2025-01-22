using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Core.Contracts
{
    internal class SecurityContract : KeystoreContract, IWrapper<Security>
    {
        private Security? _security;

        /// <inheritdoc/>
        public Security Inner => _security ??= Security.CreateNew(
            encKey: EncKey,
            macKey: MacKey,
            contentCipherId: ConfigurationDataModel.ContentCipherId,
            fileNameCipherId: ConfigurationDataModel.FileNameCipherId,
            fileNameEncodingId: ConfigurationDataModel.FileNameEncodingId);

        public SecurityContract(SecretKey encKey, SecretKey macKey, VaultKeystoreDataModel keystoreDataModel, VaultConfigurationDataModel configurationDataModel)
            : base(encKey, macKey, keystoreDataModel, configurationDataModel)
        {
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _security?.Dispose();
            base.Dispose();
        }
    }
}
