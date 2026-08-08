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
        protected IVfsRoot? StorageRoot { get; private set; }

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
            if (StorageRoot.PlaintextRoot is not IModifiableFolder modifiableFolder)
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
            if (StorageRoot.PlaintextRoot is not IModifiableFolder modifiableFolder)
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

        protected async Task Base_WriteAsync_LargeFile_ReadAsync_SameContent_NoThrow()
        {
            ArgumentNullException.ThrowIfNull(StorageRoot);

            // Arrange
            var data = new byte[300_000];
            Random.Shared.NextBytes(data);
            if (StorageRoot.PlaintextRoot is not IModifiableFolder modifiableFolder)
            {
                Assert.Fail($"Folder is not {nameof(IModifiableFolder)}.");
                return;
            }

            // Act
            var file = await modifiableFolder.CreateFileAsync("LARGE_FILE_ASYNC");
            await using (var stream = await file.OpenReadWriteAsync())
            {
                await stream.WriteAsync(data);
                await stream.FlushAsync();
            }

            var compareData = new byte[data.Length];
            await using (var stream = await file.OpenReadWriteAsync())
            {
                stream.Length.Should().Be(data.Length);

                var totalRead = 0;
                while (totalRead < compareData.Length)
                {
                    var read = await stream.ReadAsync(compareData.AsMemory(totalRead));
                    if (read <= 0)
                        break;

                    totalRead += read;
                }

                totalRead.Should().Be(data.Length);
            }

            // Assert
            data.SequenceEqual(compareData).Should().BeTrue();
        }

        protected async Task Base_Write_SparseFile_ReadGap_ReturnsZeros_NoThrow()
        {
            ArgumentNullException.ThrowIfNull(StorageRoot);

            // Arrange: write a marker far past EOF so the intervening chunks are never written and
            // the writer extends the ciphertext with a sparse gap.
            // Reading the gap back must return zeros without an integrity error.
            // The path guarded by the all-zero chunk check in ChunkReader.
            const long gapOffset = 200_000; // spans several plaintext chunks
            var marker = new byte[] { 1, 2, 3, 4 };
            if (StorageRoot.PlaintextRoot is not IModifiableFolder modifiableFolder)
            {
                Assert.Fail($"Folder is not {nameof(IModifiableFolder)}.");
                return;
            }

            // Act
            var file = await modifiableFolder.CreateFileAsync("SPARSE_FILE");
            await using (var stream = await file.OpenReadWriteAsync())
            {
                stream.Position = gapOffset;
                await stream.WriteAsync(marker);
                await stream.FlushAsync();
            }

            // Assert
            await using (var readStream = await file.OpenReadWriteAsync())
            {
                readStream.Length.Should().Be(gapOffset + marker.Length);

                var gap = new byte[gapOffset];
                readStream.Position = 0;
                var totalRead = 0;
                while (totalRead < gap.Length)
                {
                    var read = await readStream.ReadAsync(gap.AsMemory(totalRead));
                    if (read <= 0)
                        break;

                    totalRead += read;
                }

                totalRead.Should().Be(gap.Length);
                Array.TrueForAll(gap, static b => b == 0).Should().BeTrue();

                var readMarker = new byte[marker.Length];
                readStream.Position = gapOffset;
                var markerRead = 0;
                while (markerRead < readMarker.Length)
                {
                    var read = await readStream.ReadAsync(readMarker.AsMemory(markerRead));
                    if (read <= 0)
                        break;

                    markerRead += read;
                }

                marker.SequenceEqual(readMarker).Should().BeTrue();
            }
        }

        protected async Task Base_SetLength_Truncate_Then_Extend_ReadsZeros_NoThrow()
        {
            ArgumentNullException.ThrowIfNull(StorageRoot);

            // Arrange: truncate a file to drop a secret, then extend it again on the same handle.
            // The extended region must read as zeros. The removed plaintext must not be
            // resurrected from the chunk cache and re-encrypted back into the vault
            var secret = "SUPER_SECRET_VALUE"u8.ToArray();
            var prefix = new byte[1000];
            Random.Shared.NextBytes(prefix);
            if (StorageRoot.PlaintextRoot is not IModifiableFolder modifiableFolder)
            {
                Assert.Fail($"Folder is not {nameof(IModifiableFolder)}.");
                return;
            }

            // Act
            var file = await modifiableFolder.CreateFileAsync("TRUNCATED_FILE");
            await using (var stream = await file.OpenReadWriteAsync())
            {
                await stream.WriteAsync(prefix);
                await stream.WriteAsync(secret);
                await stream.FlushAsync();

                stream.SetLength(prefix.Length);
                stream.SetLength(prefix.Length + secret.Length);
                await stream.FlushAsync();
            }

            // Assert
            await using (var readStream = await file.OpenReadWriteAsync())
            {
                readStream.Length.Should().Be(prefix.Length + secret.Length);

                var contents = new byte[prefix.Length + secret.Length];
                readStream.Position = 0;
                var totalRead = 0;
                while (totalRead < contents.Length)
                {
                    var read = await readStream.ReadAsync(contents.AsMemory(totalRead));
                    if (read <= 0)
                        break;

                    totalRead += read;
                }

                totalRead.Should().Be(contents.Length);
                prefix.SequenceEqual(contents.Take(prefix.Length)).Should().BeTrue();
                Array.TrueForAll(contents[prefix.Length..], static b => b == 0).Should().BeTrue();
            }
        }

        protected async Task Base_Write_SmallFile_Then_WriteAgain_Read_SameContent_NoThrow()
        {
            ArgumentNullException.ThrowIfNull(StorageRoot);

            // Arrange
            const string dataString = "test";
            const string dataString2 = dataString + dataString;
            if (StorageRoot.PlaintextRoot is not IModifiableFolder modifiableFolder)
            {
                Assert.Fail($"Folder is not {nameof(IModifiableFolder)}.");
                return;
            }

            // Act
            var file = await modifiableFolder.CreateFileAsync("SMALL_FILE");
            await file.WriteTextAsync(dataString);
            await file.WriteTextAsync(dataString2);
            var compareString = await file.ReadTextAsync();

            // Assert
            dataString2.SequenceEqual(compareString).Should().BeTrue();
        }
    }
}