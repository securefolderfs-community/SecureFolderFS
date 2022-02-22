using SecureFolderFS.Core.VaultLoader.Discoverers.ConfigurationDiscovery;
using SecureFolderFS.Core.VaultLoader.Discoverers.KeystoreDiscovery;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.PasswordRequest;
using SecureFolderFS.Core.Security.EncryptionAlgorithm.Builder;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.VaultLoader.Routine
{
    /// <summary>
    /// Provides module for establishing vault load routine.
    /// <br/>
    /// <br/>
    /// This API is exposed.
    /// </summary>
    public interface IVaultLoadRoutine
    {
        IVaultLoadRoutineStep1 EstablishRoutine();
    }

    public interface IVaultLoadRoutineStep1
    {
        IVaultLoadRoutineStep2 AddVaultPath(VaultPath vaultPath, string volumeName = null, string mountLocation = null);
    }

    public interface IVaultLoadRoutineStep2
    {
        IVaultLoadRoutineStep3 AddFileOperations(IFileOperations fileOperations = null, IDirectoryOperations directoryOperations = null);
    }

    public interface IVaultLoadRoutineStep3
    {
        IVaultLoadRoutineStep4 FindConfigurationFile(bool useExternalDiscoverer = false, IVaultConfigurationDiscoverer vaultConfigurationDiscoverer = null);
    }

    public interface IVaultLoadRoutineStep4
    {
        IVaultLoadRoutineStep5 ContinueConfigurationFileInitialization();
    }

    public interface IVaultLoadRoutineStep5
    {
        IVaultLoadRoutineStep6 FindKeystoreFile(bool useExternalDiscoverer = false, IVaultKeystoreDiscoverer vaultKeystoreDiscoverer = null);
    }

    public interface IVaultLoadRoutineStep6
    {
        IVaultLoadRoutineStep7 ContinueKeystoreFileInitialization();
    }

    public interface IVaultLoadRoutineStep7
    {
        IVaultLoadRoutineStep8 AddEncryptionAlgorithmBuilder(IEncryptionAlgorithmBuilder encryptionAlgorithmBuilder = null);
    }

    public interface IVaultLoadRoutineStep8
    {
        IVaultLoadRoutineStep9 DeriveMasterKeyFromPassword(DisposablePassword disposablePassword);
    }

    public interface IVaultLoadRoutineStep9
    {
        IVaultLoadRoutineStep10 ContinueInitializationWithMasterKey();
    }

    public interface IVaultLoadRoutineStep10
    {
        IVaultLoadRoutineStep11 VerifyVaultConfiguration();
    }

    public interface IVaultLoadRoutineStep11
    {
        IVaultLoadRoutineStep12 ContinueInitialization();
    }

    public interface IVaultLoadRoutineStep12
    {
        IFinalizedVaultLoadRoutine Finalize();
    }
}
