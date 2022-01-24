using System.Text;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.Instance;
using SecureFolderFS.Core.PasswordRequest;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Routines;

namespace SecureFolderFS.CLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AsDebug();
        }

        static void AsDebug()
        {
            const string PASSWORD = "test"; // Don't worry, it's just for testing. If you use this password, change it immediately!!

            if (!Directory.Exists(PASSWORD))
            {
                Directory.CreateDirectory(PASSWORD);
                CreateVaultAtPath(new(PASSWORD), new DisposablePassword(Encoding.UTF8.GetBytes(PASSWORD))); // Never do that - security unsafe, don't init disposablePassword like that
            }

            IVaultInstance vaultInstance = GetVaultInstance(new(PASSWORD), new DisposablePassword(Encoding.UTF8.GetBytes(PASSWORD)));
            vaultInstance.SecureFolderFSInstance.StartFileSystem();
        }

        static void CreateVaultAtPath(VaultPath vaultPath, DisposablePassword password)
        {
            VaultRoutines.NewVaultCreationRoutine()
                         .EstablishRoutine()
                         .SetVaultPath(vaultPath)
                         .AddFileOperations()
                         .CreateConfigurationFile()
                         .CreateKeystoreFile()
                         .CreateContentFolder()
                         .AddEncryptionAlgorithmBuilder()
                         .InitializeKeystoreData(password)
                         .ContinueKeystoreFileInitialization()
                         .SetContentCipherScheme(ContentCipherScheme.XChaCha20_Poly1305)
                         .SetFileNameCipherScheme(FileNameCipherScheme.None)
                         .ContinueConfigurationFileInitialization()
                         .Finish()
                         .Deploy();
        }

        static IVaultInstance GetVaultInstance(VaultPath vaultPath, DisposablePassword password)
        {
            return VaultRoutines.NewVaultLoadRoutine()
                                .EstablishRoutine()
                                .AddVaultPath(vaultPath)
                                .AddFileOperations()
                                .FindConfigurationFile()
                                .ContinueConfigurationFileInitialization()
                                .FindKeystoreFile()
                                .ContinueKeystoreFileInitialization()
                                .AddEncryptionAlgorithmBuilder()
                                .DeriveMasterKeyFromPassword(password)
                                .ContinueInitializationWithMasterKey()
                                .VerifyVaultConfiguration()
                                .ContinueInitialization()
                                .Finish()
                                .ContinueWithOptionalRoutine()
                                .EstablishOptionalRoutine()
                                .AddFileSystemStatsTracker(new ConsoleLoggingFileSystemStatsTracker())
                                .AddChunkCachingStrategy(ChunkCachingStrategy.RandomAccessMemoryCache)
                                .AddFileSystemAdapterType(FileSystemAdapterType.DokanAdapter)
                                .AddFileNameCachingStrategy(FileNameCachingStrategy.RandomAccessMemoryCache)
                                .Finish()
                                .Deploy();
        }
    }
}