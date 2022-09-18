using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem.CryptFiles;

namespace SecureFolderFS.Core.CryptFiles
{
    /// <inheritdoc cref="IOpenCryptFile"/>
    internal sealed class OpenCryptFile : IOpenCryptFile
    {
        /// <inheritdoc/>
        public string CiphertextPath { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
