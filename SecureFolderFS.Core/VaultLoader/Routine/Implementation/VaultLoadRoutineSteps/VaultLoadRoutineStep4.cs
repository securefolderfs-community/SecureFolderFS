using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.Instance.Implementation;
using SecureFolderFS.Core.VaultDataStore;
using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;
using SecureFolderFS.Core.VaultLoader.VaultConfiguration;

namespace SecureFolderFS.Core.VaultLoader.Routine.Implementation.VaultLoadRoutineSteps
{
    internal sealed class VaultLoadRoutineStep4 : BaseVaultLoadRoutineStep, IVaultLoadRoutineStep4
    {
        public VaultLoadRoutineStep4(VaultInstance vaultInstance, VaultLoadDataModel vaultLoadDataModel)
            : base(vaultInstance, vaultLoadDataModel)
        {
        }

        public IVaultLoadRoutineStep5 ContinueConfigurationFileInitialization()
        {
            RawVaultConfiguration rawVaultConfiguration;
            using (vaultLoadDataModel.VaultConfigurationStream)
            {
                rawVaultConfiguration = RawVaultConfiguration.Load(vaultLoadDataModel.VaultConfigurationStream);
            }

            vaultInstance.VaultVersion = new VaultVersion(rawVaultConfiguration);
            if (!VaultVersion.IsVersionSupported(vaultInstance.VaultVersion.Version))
            {
                // Not supported
                throw new UnsupportedVaultException(vaultInstance.VaultVersion.Version);
            }

            var vaultConfigurationLoaderFactory = new VaultConfigurationLoaderFactory(vaultInstance.VaultVersion);
            IVaultConfigurationLoader vaultConfigurationLoader = vaultConfigurationLoaderFactory.GetVaultConfigurationLoader();

            vaultInstance.BaseVaultConfiguration = vaultConfigurationLoader.LoadVaultConfiguration(rawVaultConfiguration);

            return new VaultLoadRoutineStep5(vaultInstance, vaultLoadDataModel);
        }
    }
}
