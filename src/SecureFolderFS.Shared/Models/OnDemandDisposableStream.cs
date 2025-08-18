using System.IO;

namespace SecureFolderFS.Shared.Models
{
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