using NUnit.Framework;

namespace SecureFolderFS.Tests.FileSystemTests
{
    [TestFixture]
    public class ReadWriteTests : BaseReadWriteTests
    {
        [SetUp]
        public async Task Initialize()
        {
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

        [Test]
        public async Task Write_SmallFile_Then_WriteAgain_Read_SameContent_NoThrow()
        {
            await Base_Write_SmallFile_Then_WriteAgain_Read_SameContent_NoThrow();
        }
    }
}