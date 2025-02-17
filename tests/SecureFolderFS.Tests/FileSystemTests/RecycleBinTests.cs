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

        [SetUp]
        public async Task Initialize()
        {
            var vaultFileSystemService = DI.Service<IVaultFileSystemService>();
            var localFileSystem = await vaultFileSystemService.GetLocalFileSystemAsync(default);

            _storageRoot = await MountVault(localFileSystem, (nameof(FileSystemOptions.IsRecycleBinEnabled), true));
        }

        [Test]
        public async Task Create_File_Delete_ThatItem_EnumerateRecycleBin_NoThrow()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);

            // Arrange
            var recycleBinService = DI.Service<IRecycleBinService>();
            if (_storageRoot.VirtualizedRoot is not IModifiableFolder modifiableFolder)
            {
                Assert.Fail($"Folder is not {nameof(IModifiableFolder)}.");
                return;
            }

            // Act
            const string fileName = "FILE";
            var file = await modifiableFolder.CreateFileAsync(fileName);
            await modifiableFolder.DeleteAsync(file);

            // Assert
            var items = await recycleBinService.GetRecycleBinItemsAsync(_storageRoot).ToArrayAsync();
            items.First().Title.Should().BeEquivalentTo(fileName);
        }
    }
}
