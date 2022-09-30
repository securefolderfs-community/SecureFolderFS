using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem.Models;

namespace SecureFolderFS.Core.FileSystem
{
    public interface IMountable
    {
        Task<IVirtualFileSystem> MountFileSystemAsync(MountOptions mountOptions, CancellationToken cancellationToken = default);
    }
}
