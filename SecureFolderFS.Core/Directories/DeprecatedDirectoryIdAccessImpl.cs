using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Directories
{
    internal sealed class DeprecatedDirectoryIdAccessImpl : InstantDirectoryIdAccess
    {
        protected override Stream? OpenDirectoryIdStream(string ciphertextPath, FileMode mode, FileAccess access)
        {
            try
            {
                return File.Open(ciphertextPath, mode, access);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        protected override Task<Stream?> OpenDirectoryIdStreamAsync(string ciphertextPath, FileMode mode, FileAccess access, CancellationToken cancellationToken)
        {
            try
            {
                return Task.FromResult<Stream?>(File.Open(ciphertextPath, mode, access));
            }
            catch (FileNotFoundException)
            {
                return Task.FromResult<Stream?>(null);
            }
        }
    }
}
