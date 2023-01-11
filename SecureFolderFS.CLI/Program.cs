using System.Text;
using SecureFolderFS.Core.Cryptography.Enums;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.FUSE.AppModels;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Shared.Utils;
using SecureFolderFS.WinUI.ServiceImplementation;
using SecureFolderFS.WinUI.Storage.NativeStorage;

namespace SecureFolderFS.CLI
{
    internal static class Program
    {
        private static readonly string VaultFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                nameof(SecureFolderFS), "vaults/FuseTest");

        private static readonly string KeystorePath = Path.Combine(VaultFolder, "keystore.json");
        private static readonly string ConfigPath = Path.Combine(VaultFolder, "config.json");

        private static async Task Main(string[] args)
        {
            if (!Directory.Exists(VaultFolder))
                await CreateVaultAsync();
            var fs = await UnlockVaultAsync();

            Console.ReadKey();
            await fs.CloseAsync(FileSystemCloseMethod.CloseForcefully);
        }

        private static async Task CreateVaultAsync()
        {
            if (Directory.Exists(VaultFolder))
            {
                Directory.Delete(VaultFolder, true);
                await Task.Delay(1000);
            }

            Directory.CreateDirectory(VaultFolder);

            await using var keystoreStream = new FileStream(KeystorePath, FileMode.Create);
            await using var configStream = new FileStream(ConfigPath, FileMode.Create);

            using var creationRoutine = VaultRoutines.NewCreationRoutine();
            creationRoutine.SetVaultPassword(new Password());
            await creationRoutine.WriteKeystoreAsync(keystoreStream, StreamSerializer.Instance);
            await creationRoutine.WriteConfigurationAsync(
                new VaultOptions(ContentCipherScheme.XChaCha20_Poly1305, FileNameCipherScheme.AES_SIV), configStream,
                StreamSerializer.Instance);
            await creationRoutine.CreateContentFolderAsync(new NativeFolder(VaultFolder));
        }

        private static async Task<IVirtualFileSystem> UnlockVaultAsync()
        {
            await using var keystoreStream = new FileStream(KeystorePath, FileMode.Open);
            await using var configStream = new FileStream(ConfigPath, FileMode.Open);

            using var unlockRoutine = VaultRoutines.NewUnlockRoutine();
            await unlockRoutine.ReadConfigurationAsync(configStream, StreamSerializer.Instance);
            await unlockRoutine.ReadKeystoreAsync(keystoreStream, StreamSerializer.Instance);
            unlockRoutine.DeriveKeystore(new Password());
            await unlockRoutine.SetVaultStoreAsync(new NativeFolder(VaultFolder), new NativeStorageService());

            var mountableFileSystem = await unlockRoutine.PrepareAndUnlockAsync(new FileSystemOptions()
            {
                AdapterType = FileSystemAdapterType.FuseAdapter
            });

            return await mountableFileSystem.MountAsync(new FuseMountOptions());
        }

        private sealed class Password : IPassword
        {
            public void Dispose()
            {
            }

            public byte[] GetPassword()
            {
                return Encoding.UTF8.GetBytes("password");
            }
        }
    }
}
