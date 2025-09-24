using NUnit.Framework;
using SecureFolderFS.Storage.MemoryStorageEx;
using SecureFolderFS.Tests.Models;

namespace SecureFolderFS.Tests.FileSystemTests
{
    [TestFixture]
    public class ConstrainedReadWriteTests : BaseReadWriteTests
    {
        [SetUp]
        public async Task Initialize()
        {
            var vaultPath = Path.Combine(Path.DirectorySeparatorChar.ToString(), "TestVault");
            var streamSource = new ConstrainedMemoryStreamSource(false, true, true);
            var vaultFolder = new MemoryFolderEx(vaultPath, Path.GetFileName(vaultPath), null, streamSource);
            var options = new MockVaultOptions()
            {
                VaultFolder = vaultFolder
            };

            await SetupAsync(options);
        }

        [Test]
        public async Task Write_SmallFile_Read_SameContent_NoThrow()
        {
            await Base_Write_SmallFile_Read_SameContent_NoThrow();
        }

        [Test]
        public async Task Write_LargeFile_Read_SameContent_NoThrow()
        {
            await Base_Write_LargeFile_Read_SameContent_NoThrow();
        }
    }
}