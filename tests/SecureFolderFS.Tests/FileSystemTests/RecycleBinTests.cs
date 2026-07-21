using FluentAssertions;
using NUnit.Framework;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Tests.FileSystemTests
{
    [TestFixture]
    public class RecycleBinTests : BaseFileSystemTests
    {
        private IVfsRoot? _storageRoot;
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
            var modifiableFolder = _storageRoot.PlaintextRoot as IModifiableFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");

            // Act
            const string fileName = "FILE";
            var file = await modifiableFolder.CreateFileAsync(fileName);
            await modifiableFolder.DeleteAsync(file);

            // Assert
            var recycleBin = await _recycleBinService.GetOrCreateRecycleBinAsync(_storageRoot);
            var recycleBinItems = await recycleBin.GetItemsAsync().ToArrayAsyncImpl();
            recycleBinItems.First().Name.Should().BeEquivalentTo(fileName);

            Assert.Pass($"{nameof(recycleBinItems)}:\n" + string.Join('\n', recycleBinItems.Select(x => (x as IWrapper<IStorableChild>)?.Inner.Id)));
        }

        [Test]
        public async Task Create_FolderWith_SubFile_SubFolder_Delete_And_InspectRecycleBin_NoThrow()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);
            ArgumentNullException.ThrowIfNull(_recycleBinService);

            // Arrange
            var modifiableFolder = _storageRoot.PlaintextRoot as IModifiableFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");

            // Act
            var subFolder = await modifiableFolder.CreateFolderAsync("FOLDER") as IModifiableFolder;
            _ = subFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");

            var createdFile = await subFolder.CreateFileAsync("SUB_FILE");
            var createdFolder = await subFolder.CreateFolderAsync("SUB_FOLDER");

            await subFolder.DeleteAsync(createdFile);
            await subFolder.DeleteAsync(createdFolder);

            // Assert
            var recycleBin = await _recycleBinService.GetRecycleBinAsync(_storageRoot);
            var recycleBinItems = await recycleBin.GetItemsAsync().ToArrayAsyncImpl();
            var first = recycleBinItems[0];
            var second = recycleBinItems[1];

            first.Name.Should().Match(x => x == "SUB_FILE" || x == "SUB_FOLDER");
            second.Name.Should().Match(x => x == "SUB_FILE" || x == "SUB_FOLDER");

            Assert.Pass($"{nameof(recycleBinItems)}:\n" + string.Join('\n', recycleBinItems.Select(x => (x as IWrapper<IStorableChild>)?.Inner.Id)));
        }

        [Test]
        public async Task Delete_File_TracksOccupiedSize_And_ClearsOnPermanentDelete()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);
            ArgumentNullException.ThrowIfNull(_recycleBinService);

            // Arrange
            var modifiableFolder = _storageRoot.PlaintextRoot as IModifiableFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");
            var data = new byte[4096];
            Random.Shared.NextBytes(data);

            // Act
            var file = await modifiableFolder.CreateFileAsync("SIZED_FILE");
            await file.WriteBytesAsync(data);
            await modifiableFolder.DeleteAsync(file);

            // Assert - the occupied size reflects the plaintext size even with an unlimited-capacity bin
            var recycleBin = await _recycleBinService.GetOrCreateRecycleBinAsync(_storageRoot);
            var occupiedSize = await recycleBin.GetSizeAsync();
            occupiedSize.Should().Be(data.Length);

            // Act - permanently delete the recycled item
            var recycleBinItems = await recycleBin.GetItemsAsync().ToArrayAsyncImpl();
            var innerItem = (IStorableChild)recycleBinItems.First().AsWrapper<IStorable>().GetWrapperAt(1).Inner;
            await recycleBin.DeleteAsync(innerItem);

            // Assert - the occupied size returns to zero and the bin is empty
            (await recycleBin.GetSizeAsync()).Should().Be(0L);
            (await recycleBin.GetItemsAsync().ToArrayAsyncImpl()).Should().BeEmpty();
        }

        [Test]
        public async Task Delete_File_ExceedingQuota_IsDeletedPermanently()
        {
            ArgumentNullException.ThrowIfNull(_recycleBinService);
            ArgumentNullException.ThrowIfNull(_fileExplorerService);

            // Arrange - a bin with a 4KB quota
            var vaultFileSystemService = DI.Service<IVaultFileSystemService>();
            var localFileSystem = await vaultFileSystemService.GetLocalFileSystemAsync(CancellationToken.None);
            var storageRoot = await MountVault(localFileSystem, null, (nameof(FileSystemOptions.RecycleBinSize), 4096L));
            var modifiableFolder = storageRoot.PlaintextRoot as IModifiableFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");

            // Act - a small file fits the quota, a large one does not
            var smallFile = await modifiableFolder.CreateFileAsync("SMALL_FILE");
            await smallFile.WriteBytesAsync(new byte[128]);
            await modifiableFolder.DeleteAsync(smallFile);

            var largeFile = await modifiableFolder.CreateFileAsync("LARGE_FILE");
            await largeFile.WriteBytesAsync(new byte[128 * 1024]);
            await modifiableFolder.DeleteAsync(largeFile);

            // Assert - only the small file was recycled
            var recycleBin = await _recycleBinService.GetOrCreateRecycleBinAsync(storageRoot);
            var recycleBinItems = await recycleBin.GetItemsAsync().ToArrayAsyncImpl();
            recycleBinItems.Select(x => x.Name).Should().BeEquivalentTo("SMALL_FILE");
            (await recycleBin.GetSizeAsync()).Should().Be(128L);
        }

        [Test]
        public async Task Restore_File_WhenNameConflictExists_AppendsSuffix()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);
            ArgumentNullException.ThrowIfNull(_recycleBinService);
            ArgumentNullException.ThrowIfNull(_fileExplorerService);

            // Arrange
            var modifiableFolder = _storageRoot.PlaintextRoot as IModifiableFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");

            // Act - delete a file, then recreate one under the same name, then restore
            var file = await modifiableFolder.CreateFileAsync("CONFLICT");
            await modifiableFolder.DeleteAsync(file);
            _ = await modifiableFolder.CreateFileAsync("CONFLICT");

            var recycleBin = await _recycleBinService.GetRecycleBinAsync(_storageRoot);
            var recycleBinItems = await recycleBin.GetItemsAsync().ToArrayAsyncImpl();
            await recycleBin.RestoreItemsAsync([ recycleBinItems.First(x => x.Name == "CONFLICT") ], _fileExplorerService);

            // Assert - both files exist, the restored one with a suffix
            var rootItems = await modifiableFolder.GetItemsAsync().ToArrayAsyncImpl();
            rootItems.Select(x => x.Name).Should().Contain("CONFLICT");
            rootItems.Select(x => x.Name).Should().Contain("CONFLICT (1)");
        }

        [Test]
        public async Task RecalculateSizes_Matches_TrackedOccupiedSize()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);
            ArgumentNullException.ThrowIfNull(_recycleBinService);

            // Arrange - recycle a file and a folder (with contents)
            var modifiableFolder = _storageRoot.PlaintextRoot as IModifiableFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");
            var file = await modifiableFolder.CreateFileAsync("FILE");
            await file.WriteBytesAsync(new byte[1024]);

            var subFolder = await modifiableFolder.CreateFolderAsync("FOLDER") as IModifiableFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");
            var subFile = await subFolder.CreateFileAsync("SUB_FILE");
            await subFile.WriteBytesAsync(new byte[2048]);

            await modifiableFolder.DeleteAsync(file);
            await modifiableFolder.DeleteAsync((IStorableChild)subFolder);

            var recycleBin = await _recycleBinService.GetOrCreateRecycleBinAsync(_storageRoot);
            var trackedSize = await recycleBin.GetSizeAsync();

            // Act - a full recalculation must succeed and agree with the tracked value
            await _recycleBinService.RecalculateSizesAsync(_storageRoot);

            // Assert
            trackedSize.Should().Be(1024L + 2048L);
            (await recycleBin.GetSizeAsync()).Should().Be(trackedSize);
        }

        [Test]
        public async Task Delete_FolderTree_MemberByMember_FoldsIntoSingleEntry_And_Restores()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);
            ArgumentNullException.ThrowIfNull(_recycleBinService);
            ArgumentNullException.ThrowIfNull(_fileExplorerService);

            // Arrange - TREE/LEAF1 and TREE/sub/LEAF2
            var modifiableFolder = _storageRoot.PlaintextRoot as IModifiableFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");
            var treeFolder = await modifiableFolder.CreateFolderAsync("TREE") as IModifiableFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");
            var leaf1 = await treeFolder.CreateFileAsync("LEAF1");
            await leaf1.WriteBytesAsync(new byte[512]);

            var subFolder = await treeFolder.CreateFolderAsync("sub") as IModifiableFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");
            var leaf2 = await subFolder.CreateFileAsync("LEAF2");
            await leaf2.WriteBytesAsync(new byte[256]);

            // Act - simulate an OS client deleting the tree member-by-member, bottom-up
            await subFolder.DeleteAsync(leaf2);
            await treeFolder.DeleteAsync((IStorableChild)subFolder);
            await treeFolder.DeleteAsync(leaf1);
            await modifiableFolder.DeleteAsync((IStorableChild)treeFolder);

            // Assert - the fragments were folded back into a single restorable entry
            var recycleBin = await _recycleBinService.GetRecycleBinAsync(_storageRoot);
            var recycleBinItems = await recycleBin.GetItemsAsync().ToArrayAsyncImpl();
            recycleBinItems.Select(x => x.Name).Should().BeEquivalentTo("TREE");

            // The entry's size and the occupied total reflect the folded contents
            var treeEntry = (IRecycleBinItem)recycleBinItems[0];
            (await treeEntry.SizeOf.GetValueAsync()).Should().Be(512L + 256L);
            (await recycleBin.GetSizeAsync()).Should().Be(512L + 256L);

            // Act - restoring the single entry brings the whole tree back
            await recycleBin.RestoreItemsAsync([ treeEntry ], _fileExplorerService);

            // Assert - the hierarchy is reconstructed and the bin is empty
            var restoredRoot = await modifiableFolder.GetItemsAsync().ToArrayAsyncImpl();
            restoredRoot.Select(x => x.Name).Should().Contain("TREE");

            var restoredTree = restoredRoot.First(x => x.Name == "TREE") as IFolder ?? throw new ArgumentException("Restored item is not a folder.");
            var restoredTreeChildren = await restoredTree.GetItemsAsync().ToArrayAsyncImpl();
            restoredTreeChildren.Select(x => x.Name).Should().BeEquivalentTo("LEAF1", "sub");

            var restoredSub = restoredTreeChildren.First(x => x.Name == "sub") as IFolder ?? throw new ArgumentException("Restored item is not a folder.");
            var restoredSubChildren = await restoredSub.GetItemsAsync().ToArrayAsyncImpl();
            restoredSubChildren.Select(x => x.Name).Should().BeEquivalentTo("LEAF2");

            (await recycleBin.GetItemsAsync().ToArrayAsyncImpl()).Should().BeEmpty();
            (await recycleBin.GetSizeAsync()).Should().Be(0L);
        }

        [Test]
        public async Task Create_FolderWith_SubFile_SubFolder_Delete_And_Restore_NoThrow()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);
            ArgumentNullException.ThrowIfNull(_recycleBinService);
            ArgumentNullException.ThrowIfNull(_fileExplorerService);

            // Arrange
            var modifiableFolder = _storageRoot.PlaintextRoot as IModifiableFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");

            // Act
            var subFolder = await modifiableFolder.CreateFolderAsync("FOLDER") as IModifiableFolder;
            _ = subFolder ?? throw new ArgumentException($"Folder is not {nameof(IModifiableFolder)}.");

            var createdFile = await subFolder.CreateFileAsync("SUB_FILE");
            var createdFolder = await subFolder.CreateFolderAsync("SUB_FOLDER");

            await subFolder.DeleteAsync(createdFile);
            await subFolder.DeleteAsync(createdFolder);

            var recycleBin = await _recycleBinService.GetRecycleBinAsync(_storageRoot);
            var recycleBinItems = await recycleBin.GetItemsAsync().ToArrayAsyncImpl();
            var deletedFile = recycleBinItems.First(x => x.Name == "SUB_FILE");
            var deletedFolder = recycleBinItems.First(x => x.Name == "SUB_FOLDER");

            await recycleBin.RestoreItemsAsync([ deletedFile ], _fileExplorerService);
            await recycleBin.RestoreItemsAsync([ deletedFolder ], _fileExplorerService);

            // Assert
            var restoredItems = await subFolder.GetItemsAsync().ToArrayAsyncImpl();
            restoredItems.Select(x => x.Name).Should().Contain("SUB_FILE");
            restoredItems.Select(x => x.Name).Should().Contain("SUB_FOLDER");

            Assert.Pass($"{nameof(restoredItems)}:\n" + string.Join('\n', restoredItems.Select(x => x.Id)));
        }
    }
}
