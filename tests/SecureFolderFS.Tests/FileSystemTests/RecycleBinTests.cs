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
        private IFileExplorerService? _fileExplorerService;

        [SetUp]
        public async Task Initialize()
        {
            var vaultFileSystemService = DI.Service<IVaultFileSystemService>();
            var localFileSystem = await vaultFileSystemService.GetLocalFileSystemAsync(CancellationToken.None);
            
            _recycleBinService = DI.Service<IRecycleBinService>();
            _fileExplorerService = DI.Service<IFileExplorerService>();
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
            var recycleBinItems = await _recycleBinService.GetRecycleBinItemsAsync(_storageRoot).ToArrayAsync();
            recycleBinItems.First().PlaintextName.Should().BeEquivalentTo(fileName);
            
            Assert.Pass($"{nameof(recycleBinItems)}:\n" + string.Join('\n', recycleBinItems.Select(x => x.CiphertextItem.Id)));
        }

        [Test]
        public async Task Create_FolderWith_SubFile_SubFolder_Delete_And_InspectRecycleBin_NoThrow()
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
            var recycleBinItems = await _recycleBinService.GetRecycleBinItemsAsync(_storageRoot).ToArrayAsync();
            var first = recycleBinItems[0];
            var second = recycleBinItems[1];
            
            first.PlaintextName.Should().BeEquivalentTo("SUB_FILE");
            second.PlaintextName.Should().BeEquivalentTo("SUB_FOLDER");
            
            Assert.Pass($"{nameof(recycleBinItems)}:\n" + string.Join('\n', recycleBinItems.Select(x => x.CiphertextItem.Id)));
        }

        [Test]
        public async Task Create_FolderWith_SubFile_SubFolder_Delete_And_Restore_NoThrow()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);
            ArgumentNullException.ThrowIfNull(_recycleBinService);
            ArgumentNullException.ThrowIfNull(_fileExplorerService);

            // Arrange
            var modifiableFolder = _storageRoot.VirtualizedRoot as IModifiableFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");
            
            // Act
            var subFolder = await modifiableFolder.CreateFolderAsync("FOLDER") as IModifiableFolder;
            _ = subFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");

            var createdFile = await subFolder.CreateFileAsync("SUB_FILE");
            var createdFolder = await subFolder.CreateFolderAsync("SUB_FOLDER");
            
            await subFolder.DeleteAsync(createdFile);
            await subFolder.DeleteAsync(createdFolder);
            
            var recycleBinItems = await _recycleBinService.GetRecycleBinItemsAsync(_storageRoot).ToArrayAsync();
            var first = recycleBinItems[0];
            var second = recycleBinItems[1];

            await _recycleBinService.RestoreItemAsync(_storageRoot, first.CiphertextItem, _fileExplorerService);
            await _recycleBinService.RestoreItemAsync(_storageRoot, second.CiphertextItem, _fileExplorerService);
            
            // Assert
            var restoredItems = await subFolder.GetItemsAsync().ToArrayAsync();
            restoredItems.Should().HaveCount(2);
            
            Assert.Pass($"{nameof(restoredItems)}:\n" + string.Join('\n', restoredItems.Select(x => x.Id)));
        }
    }
}
