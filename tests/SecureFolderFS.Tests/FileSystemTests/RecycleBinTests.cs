using FluentAssertions;
using NUnit.Framework;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
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
            _storageRoot = await MountVault(localFileSystem, null, (nameof(FileSystemOptions.RecycleBinSize), -1L));
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
            var recycleBin = await _recycleBinService.GetOrCreateRecycleBinAsync(_storageRoot);
            var recycleBinItems = await recycleBin.GetItemsAsync().ToArrayAsync();
            recycleBinItems.First().Name.Should().BeEquivalentTo(fileName);

            Assert.Pass($"{nameof(recycleBinItems)}:\n" + string.Join('\n', recycleBinItems.Select(x => (x as IWrapper<IStorableChild>)?.Inner.Id)));
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
            var recycleBin = await _recycleBinService.GetOrCreateRecycleBinAsync(_storageRoot);
            var recycleBinItems = await recycleBin.GetItemsAsync().ToArrayAsync();
            var first = recycleBinItems[0];
            var second = recycleBinItems[1];

            first.Name.Should().Match(x => x == "SUB_FILE" || x == "SUB_FOLDER");
            second.Name.Should().Match(x => x == "SUB_FILE" || x == "SUB_FOLDER");

            Assert.Pass($"{nameof(recycleBinItems)}:\n" + string.Join('\n', recycleBinItems.Select(x => (x as IWrapper<IStorableChild>)?.Inner.Id)));
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

            var recycleBin = await _recycleBinService.GetOrCreateRecycleBinAsync(_storageRoot);
            var recycleBinItems = await recycleBin.GetItemsAsync().ToArrayAsync();
            var first = recycleBinItems[0];
            var second = recycleBinItems[1];

            await recycleBin.RestoreItemsAsync([ first ], _fileExplorerService);
            await recycleBin.RestoreItemsAsync([ second ], _fileExplorerService);

            // Assert
            var restoredItems = await subFolder.GetItemsAsync().ToArrayAsync();
            restoredItems.Should().HaveCount(2);
            restoredItems[0].Name.Should().Match(x => x == "SUB_FILE" || x == "SUB_FOLDER");
            restoredItems[1].Name.Should().Match(x => x == "SUB_FILE" || x == "SUB_FOLDER");

            Assert.Pass($"{nameof(restoredItems)}:\n" + string.Join('\n', restoredItems.Select(x => x.Id)));
        }
    }
}
