using SecureFolderFS.Core.FileSystem.Paths;
using System;

namespace SecureFolderFS.Core.Instance
{
    public interface ISecureFolderFSInstance : IDisposable
    {
        string MountLocation { get; }

        IPathConverter PathConverter { get; }

        void StartFileSystem();
    }
}
