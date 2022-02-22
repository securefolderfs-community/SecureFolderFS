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
            using (vaultCreationDataModel.VaultConfigurationStream)
            using (vaultCreationDataModel.MacKey)
            {
                const int version = VaultVersion.HIGHEST_VERSION;

                using var hmacSha256Crypt = vaultCreationDataModel.KeyCryptor.HmacSha256Crypt.GetInstance(vaultCreationDataModel.MacKey);
                hmacSha256Crypt.InitializeHMAC();
                hmacSha256Crypt.Update(BitConverter.GetBytes(version));
                hmacSha256Crypt.Update(BitConverter.GetBytes((uint)vaultCreationDataModel.FileNameCipherScheme));
                hmacSha256Crypt.DoFinal(BitConverter.GetBytes((uint)vaultCreationDataModel.ContentCipherScheme));

                var vaultConfiguration = new VaultConfiguration(
                    version,
                    vaultCreationDataModel.ContentCipherScheme,
                    vaultCreationDataModel.FileNameCipherScheme,
                    hmacSha256Crypt.GetHash());

                vaultConfiguration.WriteConfiguration(vaultCreationDataModel.VaultConfigurationStream);
            }

            return new VaultCreationRoutineStep12(vaultCreationDataModel);
        }
    }
}
