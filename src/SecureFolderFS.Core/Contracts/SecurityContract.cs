using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;

namespace SecureFolderFS.Core.Contracts
{
    internal class SecurityContract : KeystoreContract
    {
        public Security Security { get; }

        public SecurityContract(SecretKey encKey, SecretKey macKey, VaultKeystoreDataModel keystoreDataModel, VaultConfigurationDataModel configurationDataModel)
            : base(encKey, macKey, keystoreDataModel, configurationDataModel)
        {
            Security = Security.CreateNew(encKey, macKey, ConfigurationDataModel.ContentCipherId, ConfigurationDataModel.FileNameCipherId);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            Security.Dispose();
            base.Dispose();
        }
    }
}
