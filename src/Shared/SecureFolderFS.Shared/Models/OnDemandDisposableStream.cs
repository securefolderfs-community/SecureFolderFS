using System;
using System.IO;

namespace SecureFolderFS.Shared.Models
{
    [Obsolete("Use NonDisposableStream instead.")]
    public sealed class OnDemandDisposableStream : MemoryStream
    {
        public void ForceClose()
        {
            base.Dispose(true);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _ = disposing;
        }
    }
}