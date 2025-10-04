using System.Text;
using FluentAssertions;
using NUnit.Framework;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Tests.FileSystemTests
{
    [TestFixture]
    public class FileSystemCacheTests : BaseFileSystemTests
    {
        private IVFSRoot? _storageRoot;

        [SetUp]
        public async Task Initialize()
        {
            var vaultFileSystemService = DI.Service<IVaultFileSystemService>();
            var localFileSystem = await vaultFileSystemService.GetLocalFileSystemAsync(default);

            _storageRoot = await MountVault(localFileSystem, null);
        }

        [Test]
        public async Task Write_SmallStreamedFile_Then_WriteAgain_Ensure_CacheHit_NoThrow()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);

            // Arrange
            const string dataString = "test";
            if (_storageRoot.VirtualizedRoot is not IModifiableFolder modifiableFolder)
            {
                Assert.Fail($"Folder is not {nameof(IModifiableFolder)}.");
                return;
            }

            var cacheHits = 0;
            var cacheMisses = 0;
            _storageRoot.Options.FileSystemStatistics.ChunkCache = new Progress<CacheAccessType>(x =>
            {
                if (x == CacheAccessType.CacheHit)
                    cacheHits++;
                else if (x == CacheAccessType.CacheMiss)
                    cacheMisses++;
            });

            // Act
            var file = await modifiableFolder.CreateFileAsync("SMALL_FILE");
            await using var stream = await file.OpenReadWriteAsync();

            await stream.WriteAsync(Encoding.UTF8.GetBytes(dataString));
            await stream.WriteAsync(Encoding.UTF8.GetBytes(dataString));
            await stream.FlushAsync();

            // Assert
            stream.Position = 0L;
            var buffer = new byte[stream.Length];
            var read = await stream.ReadAsync(buffer);

            Console.WriteLine($"Cache Hits: {cacheHits}");
            Console.WriteLine($"Cache Misses: {cacheMisses}");

            read.Should().Be((int)stream.Length);
            Encoding.UTF8.GetString(buffer).Should().BeEquivalentTo(dataString + dataString);
            cacheMisses.Should().Be(1);
        }
    }
}