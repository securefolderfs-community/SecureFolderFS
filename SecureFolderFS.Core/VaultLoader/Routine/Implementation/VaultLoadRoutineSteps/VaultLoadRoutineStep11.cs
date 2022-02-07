using System;
using SecureFolderFS.Core.Chunks.Factory;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.Instance.Implementation;
using SecureFolderFS.Core.Security.Loader;

namespace SecureFolderFS.Core.VaultLoader.Routine.Implementation.VaultLoadRoutineSteps
{
    internal sealed class VaultLoadRoutineStep11 : BaseVaultLoadRoutineStep, IVaultLoadRoutineStep11
    {
        public VaultLoadRoutineStep11(VaultInstance vaultInstance, VaultLoadDataModel vaultLoadDataModel)
            : base(vaultInstance, vaultLoadDataModel)
        {
        }

        public IVaultLoadRoutineStep12 ContinueInitialization()
        {
            try
            {
                vaultLoadDataModel.ChunkFactory = vaultInstance.BaseVaultConfiguration.ContentCipherScheme switch
                {
                    ContentCipherScheme.AES_CTR_HMAC => new AesCtrHmacChunkFactory(),
                    ContentCipherScheme.AES_GCM => new AesGcmChunkFactory(),
                    ContentCipherScheme.XChaCha20_Poly1305 => new XChaCha20ChunkFactory(),
                    _ => throw new ArgumentOutOfRangeException(nameof(vaultInstance.BaseVaultConfiguration.ContentCipherScheme))
                };

                var securityLoaderFactory = new SecurityLoaderFactory(vaultInstance.VaultVersion, vaultLoadDataModel.ChunkFactory);
                var securityLoader = securityLoaderFactory.GetSecurityLoader();

                vaultInstance.Security = securityLoader.LoadSecurity(vaultInstance.BaseVaultConfiguration, vaultLoadDataModel.KeyCryptor, vaultLoadDataModel.MasterKey.CreateCopy());

                return new VaultLoadRoutineStep12(vaultInstance, vaultLoadDataModel);
            }
            finally
            {
                // We have created a copy of MasterKey for ISecurity, so it is safe to dispose this one
                vaultLoadDataModel?.Cleanup();
            }
        }
    }
}
