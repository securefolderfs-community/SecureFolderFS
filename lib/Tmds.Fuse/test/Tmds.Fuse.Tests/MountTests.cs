using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using static Tmds.Linux.LibC;
using Tmds.Linux;

namespace Tmds.Fuse.Tests
{
    public class MountTests
    {
        class DummyFileSystem : FuseFileSystemBase
        {
            public override int GetAttr(ReadOnlySpan<byte> path, ref stat stat, FuseFileInfoRef fiRef)
            {
                stat.st_nlink = 1;
                stat.st_mode = S_IFREG;
                return 0;
            }

            public override int Open(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
                => 0;

            public int DisposeCount { get; set; }

            public override void Dispose()
            {
                DisposeCount++;
            }
        }

        [Fact]
        public void MountFail_DisposesFileSystem_And_ThrowsFuseException()
        {
            DummyFileSystem dummyFileSystem = new DummyFileSystem();
            Assert.Throws<FuseException>(() => Fuse.Mount("/tmp/no_such_mountpoint", dummyFileSystem));
            Assert.Equal(1, dummyFileSystem.DisposeCount);
        }

        [Fact]
        public async Task Unmount_DisposesFileSystem()
        {
            DummyFileSystem dummyFileSystem = new DummyFileSystem();
            string mountPoint = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Directory.CreateDirectory(mountPoint);

            IFuseMount mount = Fuse.Mount(mountPoint, dummyFileSystem);

            bool unmounted = await mount.UnmountAsync(1000);
            Assert.True(unmounted);

            Assert.Equal(1, dummyFileSystem.DisposeCount);
        }

        [Fact]
        public async Task Unmount_Timeout()
        {
            // Mount the file system
            DummyFileSystem dummyFileSystem = new DummyFileSystem();
            string mountPoint = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(mountPoint);
            IFuseMount mount = Fuse.Mount(mountPoint, dummyFileSystem);

            // Open a file and try to unmount
            FileStream openedFile = File.OpenRead(Path.Combine(mountPoint, "filename"));
            long startTime = Stopwatch.GetTimestamp();
            const int timeout = 1000;
            bool unmounted = await mount.UnmountAsync(timeout);
            long endTime = Stopwatch.GetTimestamp();
            // Unmounting times out.
            Assert.False(unmounted);
            Assert.True((1000 * (endTime - startTime)) / Stopwatch.Frequency >= timeout);

            // Close the file and try to unmount
            openedFile.Close();
            // Unmounting succeeds.
            unmounted = await mount.UnmountAsync();
            Assert.True(unmounted);
        }
    }
}