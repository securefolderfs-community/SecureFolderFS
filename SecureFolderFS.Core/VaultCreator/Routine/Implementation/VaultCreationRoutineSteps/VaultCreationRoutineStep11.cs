using System;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultDataStore;
using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep11 : IVaultCreationRoutineStep11
    {
        private readonly VaultCreationDataModel _vaultCreationDataModel;

        public VaultCreationRoutineStep11(VaultCreationDataModel vaultCreationDataModel)
        {
            this._vaultCreationDataModel = vaultCreationDataModel;
        }

        public IVaultCreationRoutineStep12 ContinueConfigurationFileInitialization()
        {
            using (_vaultCreationDataModel.VaultConfigurationStream)
            using (_vaultCreationDataModel.MacKey)
            {
                const int version = VaultVersion.HIGHEST_VERSION;

                using var hmacSha256Crypt = _vaultCreationDataModel.KeyCryptor.HmacSha256Crypt.GetInstance(_vaultCreationDataModel.MacKey);
                hmacSha256Crypt.InitializeHMAC();
                hmacSha256Crypt.Update(BitConverter.GetBytes(version));
                hmacSha256Crypt.Update(BitConverter.GetBytes((uint)_vaultCreationDataModel.FileNameCipherScheme));
                hmacSha256Crypt.DoFinal(BitConverter.GetBytes((uint)_vaultCreationDataModel.ContentCipherScheme));

                var vaultConfiguration = new VaultConfiguration(
                    version,
                    _vaultCreationDataModel.ContentCipherScheme,
                    _vaultCreationDataModel.FileNameCipherScheme,
                    hmacSha256Crypt.GetHash());

                vaultConfiguration.WriteConfiguration(_vaultCreationDataModel.VaultConfigurationStream);
            }

            return new VaultCreationRoutineStep12(_vaultCreationDataModel);
        }
    }
}
