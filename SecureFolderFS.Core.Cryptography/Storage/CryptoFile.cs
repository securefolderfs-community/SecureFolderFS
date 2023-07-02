using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.Cryptography.Storage
{
    public class CryptoFile : IFile, IWrapper<IFile>
    {
        //private readonly IStreamsAccess _streamsAccess;
        //private readonly IPathConverter _pathConverter;
        //private readonly IDirectoryIdAccess _directoryIdAccess;

        /// <inheritdoc/>
        public IFile Inner { get; }

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public CryptoFile(IFile inner)
        {
            Inner = inner;
        }

        /// <inheritdoc/>
        public Task<Stream> OpenStreamAsync(FileAccess access, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
