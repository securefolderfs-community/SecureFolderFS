using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Security.EncryptionAlgorithm.Builder;
using SecureFolderFS.Core.VaultCreator.Generators.ConfigurationGeneration;
using SecureFolderFS.Core.VaultCreator.Generators.KeystoreGeneration;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Shared.Utils;
using System;

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

    public interface IVaultCreationRoutineStep1 : IDisposable
    {
        IVaultCreationRoutineStep2 SetVaultFolder(ILocatableFolder vaultFolder);
    }

    public interface IVaultCreationRoutineStep2 : IDisposable
    {
        IVaultCreationRoutineStep3 AddFileOperations(IFileOperations fileOperations = null, IDirectoryOperations directoryOperations = null);
    }

    public interface IVaultCreationRoutineStep3 : IDisposable
    {
        IVaultCreationRoutineStep4 CreateConfigurationFile(IVaultConfigurationGenerator vaultConfigurationGenerator = null);
    }

    public interface IVaultCreationRoutineStep4 : IDisposable
    {
        IVaultCreationRoutineStep5 CreateKeystoreFile(IVaultKeystoreGenerator vaultKeystoreGenerator = null);
    }

    public interface IVaultCreationRoutineStep5 : IDisposable
    {
        IVaultCreationRoutineStep6 CreateContentFolder();
    }

    public interface IVaultCreationRoutineStep6 : IDisposable
    {
        IVaultCreationRoutineStep7 AddEncryptionAlgorithmBuilder(IEncryptionAlgorithmBuilder encryptionAlgorithmBuilder = null);
    }

    public interface IVaultCreationRoutineStep7 : IDisposable
    {
        IVaultCreationRoutineStep8 InitializeKeystoreData(IPassword password);
    }

    public interface IVaultCreationRoutineStep8 : IDisposable
    {
        IVaultCreationRoutineStep9 ContinueKeystoreFileInitialization();
    }

    public interface IVaultCreationRoutineStep9 : IDisposable
    {
        IVaultCreationRoutineStep10 SetContentCipherScheme(ContentCipherScheme contentCipherScheme);
    }

    public interface IVaultCreationRoutineStep10 : IDisposable
    {
        IVaultCreationRoutineStep11 SetFileNameCipherScheme(FileNameCipherScheme fileNameCipherScheme);
    }

    public interface IVaultCreationRoutineStep11 : IDisposable
    {
        IVaultCreationRoutineStep12 ContinueConfigurationFileInitialization();
    }

    public interface IVaultCreationRoutineStep12 : IDisposable
    {
        IFinalizedVaultCreationRoutine Finalize();
    }
}
