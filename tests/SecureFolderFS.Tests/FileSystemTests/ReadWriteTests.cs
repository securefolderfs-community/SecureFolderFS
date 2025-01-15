using FluentAssertions;
using NUnit.Framework;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Tests.FileSystemTests
{
    [TestFixture]
    public class ReadWriteTests : BaseFileSystemTests
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
        public async Task Write_SmallFile_Read_SameContent_NoThrow()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);

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
            dataString.SequenceEqual(compareString).Should().BeTrue();
        }

        [Test]
        public async Task Write_LargeFile_Read_SameContent_NoThrow()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);

            // Arrange
            var data = new byte[300_000];
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
            data.SequenceEqual(compareData).Should().BeTrue();
        }
    }
}