using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Dokany.Callbacks;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Models;

namespace SecureFolderFS.Core.Dokany
{
    /// <inheritdoc cref="IMountableFileSystem"/>
    public sealed class DokanyMountable : IMountableFileSystem
    {
        private readonly BaseDokanyCallbacks _dokanyCallbacks;

        public DokanyMountable()
        {
            _dokanyCallbacks = new OnDeviceDokany();
        }

        /// <inheritdoc/>
        public Task<IVirtualFileSystem> MountAsync(MountOptions mountOptions, CancellationToken cancellationToken = default)
        {
            _dokanyCallbacks.
            throw new NotImplementedException();
        }
    }
}
