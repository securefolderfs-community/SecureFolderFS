namespace SecureFolderFS.Maui.AppModels
{
    internal sealed class OnDemandDisposableStream : MemoryStream
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
