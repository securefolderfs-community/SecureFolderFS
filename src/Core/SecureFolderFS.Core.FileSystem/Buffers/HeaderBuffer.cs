using SecureFolderFS.Shared.Models;
using System.Threading;

namespace SecureFolderFS.Core.FileSystem.Buffers
{
    /// <inheritdoc cref="BufferHolder"/>
    public sealed class HeaderBuffer : BufferHolder
    {
        /// <summary>
        /// Gets or sets the value that determines whether the header buffer is initialized or not.
        /// </summary>
        public bool IsHeaderReady { get; set; }

        /// <summary>
        /// Gets the synchronization root that guards header initialization.
        /// </summary>
        /// <remarks>
        /// The header buffer is shared by all streams opened on the same file,
        /// so reading or creating the header must be synchronized across streams.
        /// A <see cref="SemaphoreSlim"/> is used instead of a monitor lock
        /// so both synchronous and asynchronous code paths can participate.
        /// </remarks>
        public SemaphoreSlim SyncRoot { get; } = new(1, 1);

        public HeaderBuffer(byte[] buffer)
            : base(buffer)
        {
        }

        public HeaderBuffer(int bufferLength)
            : base(bufferLength)
        {
        }
    }
}
