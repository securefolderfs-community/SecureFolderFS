using System.Collections.Generic;

namespace SecureFolderFS.Sdk.Storage.StoragePool
{
    public static class SharedStoragePool
    {
        private static readonly List<IFilePool> _filePools = new();

        public static IReadOnlyList<IFilePool> FilePools => _filePools;

        public static void UpdateFilePool(IFilePool filePool)
        {
            if (!_filePools.Contains(filePool))
                _filePools.Add(filePool);
        }

        public static void ReturnFilePool(IFilePool filePool)
        {
            _filePools.Remove(filePool);
        }
    }
}
