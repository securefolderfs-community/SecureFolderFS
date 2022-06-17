using System.Collections.Generic;

namespace SecureFolderFS.Sdk.Storage.StoragePool
{
    public static class SharedStoragePool
    {
        private static readonly List<IFilePool> _filePools = new();
        private static readonly List<IFolderPool> _folderPools = new();

        public static IReadOnlyList<IFilePool> FilePools => _filePools;

        public static IReadOnlyList<IFolderPool> FolderPools => _folderPools;

        public static void UpdateFilePool(IFilePool filePool)
        {
            if (!_filePools.Contains(filePool))
                _filePools.Add(filePool);
        }

        public static void UpdateFolderPool(IFolderPool folderPool)
        {
            if (!_folderPools.Contains(folderPool))
                _folderPools.Add(folderPool);
        }

        public static bool ReturnFilePool(IFilePool filePool)
        {
            return _filePools.Remove(filePool);
        }

        public static bool ReturnFilePool(IFolderPool folderPool)
        {
            return _folderPools.Remove(folderPool);
        }
    }
}
