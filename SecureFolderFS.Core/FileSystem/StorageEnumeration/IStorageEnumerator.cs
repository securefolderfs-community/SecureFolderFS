using System;
using System.Collections.Generic;

namespace SecureFolderFS.Core.FileSystem.StorageEnumeration
{
    /// <summary>
    /// Provides module for storage enumeration.
    /// <br/>
    /// <br/>
    /// This API is exposed.
    /// </summary>
    public interface IStorageEnumerator
    {
        IEnumerable<FileEnumerationInfo> EnumerateFileSystemEntries(string path, string searchPattern);

        FileEnumerationInfo GetFileInfo(string path);
    }
}
