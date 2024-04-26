using System;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    public interface IReadWriteStatistics
    {
        /// <summary>
        /// Gets the <see cref="IProgress{T}"/> that reports the number of bytes read.
        /// </summary>
        IProgress<long>? BytesRead { get; set; }

        /// <summary>
        /// Gets the <see cref="IProgress{T}"/> that reports the number of bytes written.
        /// </summary>
        IProgress<long>? BytesWritten { get; set; }
    }
}
