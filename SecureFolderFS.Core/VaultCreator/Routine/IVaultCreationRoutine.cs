using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.PasswordRequest;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Security.EncryptionAlgorithm.Builder;
using SecureFolderFS.Core.VaultCreator.Generators.ConfigurationGeneration;
using SecureFolderFS.Core.VaultCreator.Generators.KeystoreGeneration;

namespace SecureFolderFS.Core.VaultCreator.Routine
{
    /// <summary>
    /// Provides module for establishing vault creation routine.
    /// <br/>
    /// <br/>
    /// This API is exposed.
    /// </summary>
    public interface IVaultCreationRoutine
    {
        IVaultCreationRoutineStep1 EstablishRoutine();
    }

    public interface IVaultCreationRoutineStep1
    {
        IVaultCreationRoutineStep2 SetVaultPath(VaultPath vaultPath);
    }

    public interface IVaultCreationRoutineStep2
    {
        IVaultCreationRoutineStep3 AddFileOperations(IFileOperations fileOperations = null, IDirectoryOperations directoryOperations = null);
    }

    public interface IVaultCreationRoutineStep3
    {
        IVaultCreationRoutineStep4 CreateConfigurationFile(IVaultConfigurationGenerator vaultConfigurationGenerator = null);
    }

    public interface IVaultCreationRoutineStep4
    {
        IVaultCreationRoutineStep5 CreateKeystoreFile(IVaultKeystoreGenerator vaultKeystoreGenerator = null);
    }

    public interface IVaultCreationRoutineStep5
    {
        IVaultCreationRoutineStep6 CreateContentFolder();
    }

    public interface IVaultCreationRoutineStep6
    {
        IVaultCreationRoutineStep7 AddEncryptionAlgorithmBuilder(IEncryptionAlgorithmBuilder encryptionAlgorithmBuilder = null);
    }

    public interface IVaultCreationRoutineStep7
    {
        IVaultCreationRoutineStep8 InitializeKeystoreData(DisposablePassword disposablePassword);
    }

    public interface IVaultCreationRoutineStep8
    {
        IVaultCreationRoutineStep9 ContinueKeystoreFileInitialization();
    }

    public interface IVaultCreationRoutineStep9
    {
        IVaultCreationRoutineStep10 SetContentCipherScheme(ContentCipherScheme contentCipherScheme);
    }

    public interface IVaultCreationRoutineStep10
    {
        IVaultCreationRoutineStep11 SetFileNameCipherScheme(FileNameCipherScheme fileNameCipherScheme);
    }

    public interface IVaultCreationRoutineStep11
    {
        IVaultCreationRoutineStep12 ContinueConfigurationFileInitialization();
    }

    public interface IVaultCreationRoutineStep12
    {
        IFinalizedVaultCreationRoutine Finish();
    }
}
