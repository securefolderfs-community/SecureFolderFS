using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Tests.FileSystemTests
{
    [TestClass]
    public class ReadWriteTests : BaseFileSystemTests
    {
        private IVFSRoot? _storageRoot;

        [ClassInitialize]
        public async Task Initialize()
        {
            var vaultFileSystemService = DI.Service<IVaultFileSystemService>();
            var localFileSystem = await vaultFileSystemService.GetLocalFileSystemAsync(default);

            _storageRoot = await MountVault(localFileSystem);
        }

        [TestMethod]
        public async Task Write_SmallFile_Read_SameContent_NoThrow()
        {
            Assert.IsNotNull(_storageRoot);

            // Arrange
            const string dataString = "test";
            if (_storageRoot.Inner is not IModifiableFolder modifiableFolder)
            {
                Assert.Fail($"Folder is not {nameof(IModifiableFolder)}.");
                return;
            }

            // Act
            var file = await modifiableFolder.CreateFileAsync("SMALL_FILE");
            await file.WriteTextAsync(dataString);
            var compareString = await file.ReadTextAsync();

            // Assert
            Assert.IsTrue(dataString.SequenceEqual(compareString));
        }

        [TestMethod]
        public async Task Write_LargeFile_Read_SameContent_NoThrow()
        {
            Assert.IsNotNull(_storageRoot);

            // Arrange
            var data = new byte[50_000];
            Random.Shared.NextBytes(data);
            if (_storageRoot.Inner is not IModifiableFolder modifiableFolder)
            {
                Assert.Fail($"Folder is not {nameof(IModifiableFolder)}.");
                return;
            }

            // Act
            var file = await modifiableFolder.CreateFileAsync("LARGE_FILE");
            await file.WriteBytesAsync(data);
            var compareData = await file.ReadBytesAsync(default);

            // Assert
            Assert.IsTrue(data.SequenceEqual(compareData));
        }
    }
}
