using FluentAssertions;
using NUnit.Framework;
using SecureFolderFS.Core.MacFuse;
using SecureFolderFS.Storage.SystemStorageEx;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.Tests.Models;

namespace SecureFolderFS.Tests.FileSystemTests
{
    /// <summary>
    /// End-to-end tests which mount a real vault through macFUSE and exercise it with regular file APIs.
    /// </summary>
    [TestFixture]
    [Platform("MacOsX")]
    public class MacFuseTests : BaseFileSystemTests
    {
        private IVfsRoot? _storageRoot;
        private string? _vaultPath;

        [SetUp]
        public async Task Initialize()
        {
            if (OperatingSystem.IsMacOS() && FuseSharp.Fuse.CheckDependencies())
            {
                // The FUSE file system operates on raw file system paths,
                // so the vault must reside on disk instead of in memory
                _vaultPath = Directory.CreateTempSubdirectory("MacFuseTests_").FullName;
                _storageRoot = await MountVault(new MacFuseFileSystem(), new MockVaultOptions()
                {
                    VaultFolder = new SystemFolderEx(_vaultPath)
                });
            }
            else
                Assert.Ignore("macFUSE is not installed.");

            // The mount point must be a real mounted volume. Without this the tests would
            // still pass by reading and writing the raw (unmounted) mount point directory
            var mountPoint = _storageRoot.VirtualizedRoot.Id;
            DriveInfo.GetDrives().Should().Contain(x => x.RootDirectory.FullName.TrimEnd('/') == mountPoint.TrimEnd('/'));
        }

        [TearDown]
        public async Task Cleanup()
        {
            if (_storageRoot is null)
                return;

            var mountPoint = _storageRoot.VirtualizedRoot.Id;
            await _storageRoot.DisposeAsync();

            // After a clean unmount the empty mount point directory is removed. Files written
            // past the file system (plaintext leakage) would leave the directory behind
            DriveInfo.GetDrives().Should().NotContain(x => x.RootDirectory.FullName.TrimEnd('/') == mountPoint.TrimEnd('/'));
            Directory.Exists(mountPoint).Should().BeFalse();

            if (_vaultPath is not null)
                Directory.Delete(_vaultPath, recursive: true);
        }

        [Test]
        public async Task Mount_WriteFile_ReadBack_ThroughKernel()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);
            var mountPoint = _storageRoot.VirtualizedRoot.Id;

            Directory.Exists(mountPoint).Should().BeTrue();

            var filePath = Path.Combine(mountPoint, "hello.txt");
            await File.WriteAllTextAsync(filePath, "Hello from macFUSE!");

            var contents = await File.ReadAllTextAsync(filePath);
            contents.Should().Be("Hello from macFUSE!");
        }

        [Test]
        public async Task Mount_CreateDirectory_Enumerate_Rename_Delete()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);
            var mountPoint = _storageRoot.VirtualizedRoot.Id;

            var directoryPath = Path.Combine(mountPoint, "folder");
            Directory.CreateDirectory(directoryPath);

            var filePath = Path.Combine(directoryPath, "data.bin");
            var payload = Enumerable.Range(0, 1024 * 128).Select(static i => (byte)i).ToArray();
            await File.WriteAllBytesAsync(filePath, payload);

            (await File.ReadAllBytesAsync(filePath)).Should().Equal(payload);
            Directory.EnumerateFiles(directoryPath).Select(Path.GetFileName).Should().ContainSingle(static x => x == "data.bin");

            var renamedPath = Path.Combine(directoryPath, "renamed.bin");
            File.Move(filePath, renamedPath);
            File.Exists(renamedPath).Should().BeTrue();
            File.Exists(filePath).Should().BeFalse();

            File.Delete(renamedPath);
            Directory.Delete(directoryPath);
            Directory.Exists(directoryPath).Should().BeFalse();
        }

        [Test]
        public async Task Mount_FinderCopy_PreservesFinderInfoAndContent()
        {
            // Regression for the Finder "error -50 / zero-byte copy" bug. macFUSE's kernel extension
            // tags the com.apple.FinderInfo setxattr of a Finder drag-copy with internal VFS flags
            // (XATTR_NOSECURITY/XATTR_NODEFAULT). The setxattr(2) syscall rejects those with EINVAL, so
            // re-issuing them verbatim on the backing file made Finder report -50 and (depending on
            // copyfile's ordering) leave the copied file empty. Only a real Finder copy reproduces the
            // exact flags, so this drives Finder via AppleScript.
            ArgumentNullException.ThrowIfNull(_storageRoot);
            var mountPoint = _storageRoot.VirtualizedRoot.Id;

            var payload = Enumerable.Range(0, 100_000).Select(static i => (byte)(i % 251)).ToArray();
            var src = Path.Combine(Path.GetTempPath(), $"macfuse-finder-{Guid.NewGuid():N}.bin");
            await File.WriteAllBytesAsync(src, payload);

            // Give the source a non-zero FinderInfo so Finder copies a meaningful one (an all-zero
            // FinderInfo is treated as absent by macOS and would not be persisted)
            var finderInfo = Enumerable.Range(1, 32).Select(static i => (byte)i).ToArray();
            SetXAttr(src, "com.apple.FinderInfo", finderInfo);

            try
            {
                var script = $"tell application \"Finder\" to duplicate (POSIX file \"{src}\") to (POSIX file \"{mountPoint}\")";
                var p = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
                    "/usr/bin/osascript", ["-e", script]) { RedirectStandardError = true, RedirectStandardOutput = true })!;
                await p.WaitForExitAsync();
                var stderr = (await p.StandardError.ReadToEndAsync()).Trim();

                var dest = Path.Combine(mountPoint, Path.GetFileName(src));

                // Only skip when Finder automation is genuinely blocked (TCC). A copy error such as
                // "-50" is the regression itself and must fail the test, not skip it.
                var automationBlocked = stderr.Contains("-1743") || stderr.Contains("Not authorized")
                                        || stderr.Contains("assistive access") || stderr.Contains("isn't running");
                if (automationBlocked)
                    Assert.Ignore($"Finder automation unavailable: '{stderr}'.");

                p.ExitCode.Should().Be(0, $"Finder copy into the vault must succeed, but failed: '{stderr}'");
                File.Exists(dest).Should().BeTrue("the Finder-copied file must exist");

                // The data fork must be fully copied, not left empty
                new FileInfo(dest).Length.Should().Be(payload.Length, "the Finder-copied file must not be left empty");
                (await File.ReadAllBytesAsync(dest)).Should().Equal(payload);

                // Finder writes com.apple.FinderInfo during the copy - the setxattr that failed with
                // EINVAL (Finder error -50) before the flag mask was applied
                var destFinderInfo = new byte[32];
                GetXAttr(dest, "com.apple.FinderInfo", destFinderInfo)
                    .Should().Be(32, "Finder must be able to write FinderInfo on the copied file");
                destFinderInfo.Should().Equal(finderInfo);

                File.Delete(dest);
            }
            finally
            {
                File.Delete(src);
            }
        }

        [System.Runtime.InteropServices.DllImport("libSystem.dylib", EntryPoint = "getxattr", CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]
        private static extern nint NativeGetXAttr(string path, string name, byte[] value, nuint size, uint position, int options);

        [System.Runtime.InteropServices.DllImport("libSystem.dylib", EntryPoint = "setxattr", CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]
        private static extern int NativeSetXAttr(string path, string name, byte[] value, nuint size, uint position, int options);

        private static int GetXAttr(string path, string name, byte[] value) => (int)NativeGetXAttr(path, name, value, (nuint)value.Length, 0, 0);

        private static int SetXAttr(string path, string name, byte[] value) => NativeSetXAttr(path, name, value, (nuint)value.Length, 0, 0);

        [Test]
        public async Task Mount_Truncate_And_Append()
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);
            var mountPoint = _storageRoot.VirtualizedRoot.Id;

            var filePath = Path.Combine(mountPoint, "resize.txt");
            await File.WriteAllTextAsync(filePath, "0123456789");

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                stream.SetLength(5);

            (await File.ReadAllTextAsync(filePath)).Should().Be("01234");

            await File.AppendAllTextAsync(filePath, "ABC");
            (await File.ReadAllTextAsync(filePath)).Should().Be("01234ABC");

            File.Delete(filePath);
        }
    }
}
