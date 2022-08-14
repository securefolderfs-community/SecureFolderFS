using System;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Instance.Implementation;
using SecureFolderFS.Core.VaultLoader.KeyDerivation;
using SecureFolderFS.Core.VaultLoader.VaultKeystore;

namespace SecureFolderFS.Core.VaultLoader.Routine.Implementation.VaultLoadRoutineSteps
{
    internal sealed class VaultLoadRoutineStep6 : BaseVaultLoadRoutineStep, IVaultLoadRoutineStep6
    {
        public VaultLoadRoutineStep6(VaultInstance vaultInstance, VaultLoadDataModel vaultLoadDataModel)
            : base(vaultInstance, vaultLoadDataModel)
        {
        }

        public IVaultLoadRoutineStep7 ContinueKeystoreFileInitialization()
        {
            var vaultKeystoreLoaderFactory = new VaultKeystoreLoaderFactory(vaultInstance.VaultVersion);
            var vaultKeystoreLoader = vaultKeystoreLoaderFactory.GetVaultKeystoreLoader();
            vaultLoadDataModel.BaseVaultKeystore = vaultKeystoreLoader.LoadVaultKeystore(vaultLoadDataModel.VaultKeystoreStream);

            if (!vaultLoadDataModel.BaseVaultKeystore.IsKeystoreValid())
            {
                throw new ArgumentException("The keystore is invalid.");
            }

            var masterKeyDerivationFactory = new MasterKeyDerivationFactory(vaultInstance.VaultVersion);
            vaultLoadDataModel.MasterKeyDerivation = masterKeyDerivationFactory.GetMasterKeyDerivation();

            return new VaultLoadRoutineStep7(vaultInstance, vaultLoadDataModel);
        }
    }
}
