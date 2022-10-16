using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Directories
{
    internal sealed class DeprecatedDirectoryIdAccessImpl : InstantDirectoryIdAccess
    {
        protected override Stream? OpenDirectoryIdStream(string ciphertextPath, FileAccess access)
        {
            try
            {
                return File.Open(ciphertextPath, FileMode.Open, access);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        protected override Task<Stream?> OpenDirectoryIdStreamAsync(string ciphertextPath, FileAccess access, CancellationToken cancellationToken)
        {
            try
            {
                return Task.FromResult<Stream?>(File.Open(ciphertextPath, FileMode.Open, access));
            }
            catch (FileNotFoundException)
            {
                return Task.FromResult<Stream?>(null);
            }
        }
    }
}
