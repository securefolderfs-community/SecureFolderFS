using NUnit.Framework;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Tests.FileSystemTests
{
    [TestFixture]
    public class FileStructureTests : BaseFileSystemTests
    {
        private IVFSRoot? _storageRoot;

        [SetUp]
        public async Task Initialize()
        {
            var vaultFileSystemService = DI.Service<IVaultFileSystemService>();
            var localFileSystem = await vaultFileSystemService.GetLocalFileSystemAsync(default);

            _storageRoot = await MountVault(localFileSystem);
        }

        [Test]
        public async Task Create_FolderWith_SubFile_SubFolder_Delete_EachItem_ShouldThrow()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);

            // Arrange
            if (_storageRoot.Inner is not IModifiableFolder modifiableFolder)
            {
                Assert.Fail($"Folder is not {nameof(IModifiableFolder)}.");
                return;
            }

            // Act
            var subFolder = await modifiableFolder.CreateFolderAsync("FOLDER") as IModifiableFolder;
            _ = subFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");

            var createdFile = await subFolder.CreateFileAsync("SUB_FILE");
            var createdFolder = await subFolder.CreateFolderAsync("SUB_FOLDER");

            await subFolder.DeleteAsync(createdFile);
            await subFolder.DeleteAsync(createdFolder);

            // Assert
            Assert.ThrowsAsync<FileNotFoundException>(async () => await subFolder.GetFirstByNameAsync("SUB_FILE"));
            Assert.ThrowsAsync<FileNotFoundException>(async () => await subFolder.GetFirstByNameAsync("SUB_FOLDER"));
        }

        [Test]
        public async Task Create_FolderWith_SubFile_SubFolder_Delete_Folder_ShouldThrow()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);

            // Arrange
            if (_storageRoot.Inner is not IModifiableFolder modifiableFolder)
            {
                Assert.Fail($"Folder is not {nameof(IModifiableFolder)}.");
                return;
            }

            // Act
            var subFolder = await modifiableFolder.CreateFolderAsync("FOLDER") as IModifiableFolder;
            _ = subFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");

            _ = await subFolder.CreateFileAsync("SUB_FILE");
            _ = await subFolder.CreateFolderAsync("SUB_FOLDER");

            await modifiableFolder.DeleteAsync((IStorableChild)subFolder);

            // Assert
            Assert.ThrowsAsync<FileNotFoundException>(async () => await modifiableFolder.GetFirstByNameAsync("FOLDER"));
        }
    }
}
