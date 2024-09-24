using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using System;

namespace SecureFolderFS.Core.Contracts
{
    internal class KeystoreContract : IDisposable
    {
        public SecretKey EncKey { get; }
        
        public SecretKey MacKey { get; }

        public VaultKeystoreDataModel KeystoreDataModel { get; }

        public VaultConfigurationDataModel ConfigurationDataModel { get; }

        public KeystoreContract(SecretKey encKey, SecretKey macKey, VaultKeystoreDataModel keystoreDataModel, VaultConfigurationDataModel configurationDataModel)
        {
            EncKey = encKey;
            MacKey = macKey;
            KeystoreDataModel = keystoreDataModel;
            ConfigurationDataModel = configurationDataModel;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Convert.ToBase64String(EncKey)}{Constants.KEY_TEXT_SEPARATOR}{Convert.ToBase64String(MacKey)}";
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            EncKey.Dispose();
            MacKey.Dispose();
        }
    }
}
