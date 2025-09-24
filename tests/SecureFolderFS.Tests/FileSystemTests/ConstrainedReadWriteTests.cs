using NUnit.Framework;
using SecureFolderFS.Storage.MemoryStorageEx;

namespace SecureFolderFS.Tests.FileSystemTests
{
    [TestFixture]
    public class ConstrainedReadWriteTests : BaseReadWriteTests
    {
        [SetUp]
        public async Task Initialize()
        {
            var options = new MockVaultOptions()
            {
                VaultFolder = new MemoryFolderEx()
            };
            await SetupAsync(null);
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