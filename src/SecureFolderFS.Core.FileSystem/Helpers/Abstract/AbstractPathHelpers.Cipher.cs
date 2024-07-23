using OwlCore.Storage;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Helpers.Abstract
{
    /// <summary>
    /// A set of file system path management helpers that work on any platform including constrained environments with limited file system access.
    /// </summary>
    public static partial class AbstractPathHelpers
    {
        public static Task<string> GetCiphertextPathAsync(IStorableChild plaintextStorable, FileSystemSpecifics specifics)
        {
            throw new NotImplementedException();
        }

        public static Task<string> GetPlaintextPathAsync(IStorableChild ciphertextStorable, FileSystemSpecifics specifics)
        {
            throw new NotImplementedException();
        }
    }
}
