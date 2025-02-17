using FluentAssertions;
using NUnit.Framework;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Tests.FileSystemTests
{
    [TestFixture]
    public class RecycleBinTests : BaseFileSystemTests
    {
        private IVFSRoot? _storageRoot;
        private IRecycleBinService? _recycleBinService;

        [SetUp]
        public async Task Initialize()
        {
            var vaultFileSystemService = DI.Service<IVaultFileSystemService>();
            var localFileSystem = await vaultFileSystemService.GetLocalFileSystemAsync(CancellationToken.None);
            
            _recycleBinService = DI.Service<IRecycleBinService>();
            _storageRoot = await MountVault(localFileSystem, (nameof(FileSystemOptions.IsRecycleBinEnabled), true));
        }

        [Test]
        public async Task Create_File_Delete_ThatFile_InspectRecycleBin_NoThrow()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);
            ArgumentNullException.ThrowIfNull(_recycleBinService);

            // Arrange
            var modifiableFolder = _storageRoot.VirtualizedRoot as IModifiableFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");

            // Act
            const string fileName = "FILE";
            var file = await modifiableFolder.CreateFileAsync(fileName);
            await modifiableFolder.DeleteAsync(file);

            // Assert
            var items = await _recycleBinService.GetRecycleBinItemsAsync(_storageRoot).ToArrayAsync();
            items.First().Title.Should().BeEquivalentTo(fileName);
        }

        [Test]
        public async Task Create_FolderWith_SubFile_Delete_BaseFolder_InspectRecycleBin_NoThrow()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);
            ArgumentNullException.ThrowIfNull(_recycleBinService);

            // Arrange
            var modifiableFolder = _storageRoot.VirtualizedRoot as IModifiableFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");
            
            // Act
            var subFolder = await modifiableFolder.CreateFolderAsync("FOLDER") as IModifiableFolder;
            _ = subFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");

            var createdFile = await subFolder.CreateFileAsync("SUB_FILE");
            await subFolder.DeleteAsync(createdFile);
            
            // Assert
            var items = await _recycleBinService.GetRecycleBinItemsAsync(_storageRoot).ToArrayAsync();
            items.First().Title.Should().BeEquivalentTo("SUB_FILE");
        }

        [Test]
        public async Task Create_FolderWith_SubFile_SubFolder_Delete_EachItem_InspectRecycleBin_NoThrow()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);
            ArgumentNullException.ThrowIfNull(_recycleBinService);

            // Arrange
            var modifiableFolder = _storageRoot.VirtualizedRoot as IModifiableFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");

            // Act
            var subFolder = await modifiableFolder.CreateFolderAsync("FOLDER") as IModifiableFolder;
            _ = subFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");

            var createdFile = await subFolder.CreateFileAsync("SUB_FILE");
            var createdFolder = await subFolder.CreateFolderAsync("SUB_FOLDER");

            await subFolder.DeleteAsync(createdFile);
            await subFolder.DeleteAsync(createdFolder);

            // Assert
            var items = await _recycleBinService.GetRecycleBinItemsAsync(_storageRoot).ToArrayAsync();
            items.Should().NotBeEmpty();
        }
    }
}
