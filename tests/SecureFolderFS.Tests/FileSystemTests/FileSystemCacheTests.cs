using System;
using System.Text;
using System.Threading;
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
        private IVfsRoot? _storageRoot;

        [SetUp]
        public async Task Initialize()
        {
            var vaultFileSystemService = DI.Service<IVaultFileSystemService>();
            var localFileSystem = await vaultFileSystemService.GetLocalFileSystemAsync();

            _storageRoot = await MountVault(localFileSystem, null);
        }

        [Test]
        public async Task Write_SmallStreamedFile_Then_WriteAgain_Ensure_CacheHit_NoThrow()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);

            // Arrange
            const string dataString = "test";
            if (_storageRoot.PlaintextRoot is not IModifiableFolder modifiableFolder)
            {
                Assert.Fail($"Folder is not {nameof(IModifiableFolder)}.");
                return;
            }

            var cacheHits = 0;
            var cacheMisses = 0;
            _storageRoot.Options.FileSystemStatistics.ChunkCache = new InlineProgress<CacheAccessType>(x =>
            {
                if (x == CacheAccessType.CacheHit)
                    Interlocked.Increment(ref cacheHits);
                else if (x == CacheAccessType.CacheMiss)
                    Interlocked.Increment(ref cacheMisses);
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

        private sealed class InlineProgress<T> : IProgress<T>
        {
            private readonly Action<T> _handler;

            public InlineProgress(Action<T> handler)
            {
                _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            }

            public void Report(T value) => _handler(value);
        }
    }
}
