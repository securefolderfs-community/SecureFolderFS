using System;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultDataStore;
using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;

namespace SecureFolderFS.Core.VaultCreator.Routine.Implementation.VaultCreationRoutineSteps
{
    internal sealed class VaultCreationRoutineStep11 : BaseVaultCreationRoutineStep, IVaultCreationRoutineStep11
    {
        public VaultCreationRoutineStep11(VaultCreationDataModel vaultCreationDataModel)
            : base(vaultCreationDataModel)
        {
        }

        public IVaultCreationRoutineStep12 ContinueConfigurationFileInitialization()
        {
            using (vaultCreationDataModel.MacKey)
            {
                const int version = VaultVersion.HIGHEST_VERSION;

                using var hmacSha256Crypt = vaultCreationDataModel.KeyCryptor.GetHmacInstance();
                hmacSha256Crypt.InitializeHmac(vaultCreationDataModel.MacKey);
                hmacSha256Crypt.Update(BitConverter.GetBytes(version));
                hmacSha256Crypt.Update(BitConverter.GetBytes((uint)vaultCreationDataModel.FileNameCipherScheme));
                hmacSha256Crypt.DoFinal(BitConverter.GetBytes((uint)vaultCreationDataModel.ContentCipherScheme));

                var hmacsha256Mac = new byte[Constants.Security.EncryptionAlgorithm.HmacSha256.MAC_SIZE];
                hmacSha256Crypt.GetHash(hmacsha256Mac);

                var vaultConfiguration = new VaultConfiguration(
                    version,
                    vaultCreationDataModel.ContentCipherScheme,
                    vaultCreationDataModel.FileNameCipherScheme,
                    hmacsha256Mac);

                vaultConfiguration.WriteConfiguration(vaultCreationDataModel.VaultConfigurationStream);
            }

            return new VaultCreationRoutineStep12(vaultCreationDataModel);
        }
    }
}
