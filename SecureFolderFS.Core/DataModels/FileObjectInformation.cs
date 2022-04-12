using System.IO;
using SecureFolderFS.Sdk.Paths;

namespace SecureFolderFS.Core.DataModels
{
    internal sealed class FileObjectInformation
    {
        public ICleartextPath CleartextPath { get; init; }

        public ICiphertextPath CiphertextPath { get; init; }

        public FileAttributes ActualAttributes { get; init; }

        public bool IsCoreFile { get; init; }
         
        public bool PathExists { get; init; }

        public bool PathIsDirectory { get; init; }
    }
}
