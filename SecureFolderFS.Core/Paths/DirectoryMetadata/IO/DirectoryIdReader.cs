using System.IO;
using SecureFolderFS.Core.FileSystem.Operations;

namespace SecureFolderFS.Core.Paths.DirectoryMetadata.IO
{
    internal sealed class DirectoryIdReader : IDirectoryIdReader
    {
        private readonly IFileOperations _fileOperations;

        public DirectoryIdReader(IFileOperations fileOperations)
        {
            _fileOperations = fileOperations;
        }

        public DirectoryId ReadDirectoryId(string ciphertextPath)
        {
            if (string.IsNullOrEmpty(ciphertextPath))
            {
                return DirectoryId.GetEmpty();
            }
            else if (!_fileOperations.Exists(ciphertextPath))
            {
                // TODO: Report that the folder metadata file does not exist to the HealthAPI
                return DirectoryId.CreateNew();
            }
            else
            {
                using var fileStream = _fileOperations.OpenFile(ciphertextPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return DirectoryId.FromFileStream(fileStream);
            }
        }
    }
}
