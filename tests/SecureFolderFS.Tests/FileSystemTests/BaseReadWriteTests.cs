using FluentAssertions;
using NUnit.Framework;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.Tests.Models;

namespace SecureFolderFS.Tests.FileSystemTests
{
    public abstract class BaseReadWriteTests : BaseFileSystemTests
    {
        protected IVFSRoot? StorageRoot { get; private set; }

        protected async Task SetupAsync(MockVaultOptions? options, CancellationToken cancellationToken = default)
        {
            var vaultFileSystemService = DI.Service<IVaultFileSystemService>();
            var localFileSystem = await vaultFileSystemService.GetLocalFileSystemAsync(default);

            StorageRoot = await MountVault(localFileSystem, options);
        }

        protected async Task Base_Write_SmallFile_Read_SameContent_NoThrow()
        {
            ArgumentNullException.ThrowIfNull(StorageRoot);

            // Arrange
            const string dataString = "test";
            if (StorageRoot.VirtualizedRoot is not IModifiableFolder modifiableFolder)
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

        protected async Task Base_Write_LargeFile_Read_SameContent_NoThrow()
        {
            ArgumentNullException.ThrowIfNull(StorageRoot);

            // Arrange
            var data = new byte[300_000];
            Random.Shared.NextBytes(data);
            if (StorageRoot.VirtualizedRoot is not IModifiableFolder modifiableFolder)
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