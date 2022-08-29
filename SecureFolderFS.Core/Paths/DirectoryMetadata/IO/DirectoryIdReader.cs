using System.IO;

namespace SecureFolderFS.Core.Paths.DirectoryMetadata.IO
{
    internal sealed class DirectoryIdReader : IDirectoryIdReader
    {
        public DirectoryId ReadDirectoryId(string ciphertextPath)
        {
            if (string.IsNullOrEmpty(ciphertextPath))
            {
                return DirectoryId.GetEmpty();
            }
            else if (!File.Exists(ciphertextPath))
            {
                // TODO: Report that the folder metadata file does not exist to the HealthAPI
                return DirectoryId.CreateNew();
            }
            else
            {
                using var fileStream = File.Open(ciphertextPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return DirectoryId.FromFileStream(fileStream);
            }
        }
    }
}
