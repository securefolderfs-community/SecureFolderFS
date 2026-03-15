namespace SecureFolderFS.Sdk.Dropbox.Streams
{
    /// <summary>
    /// A stream that owns a resource and disposes it alongside itself.
    /// Used to keep the Dropbox download response alive while its stream is being read.
    /// </summary>
    internal sealed class OwningStream(Stream inner, IDisposable owner) : Stream
    {
        /// <inheritdoc/>
        public override bool CanRead => inner.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => inner.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => inner.CanWrite;

        /// <inheritdoc/>
        public override long Length => inner.Length;

        /// <inheritdoc/>
        public override long Position { get => inner.Position; set => inner.Position = value; }

        /// <inheritdoc/>
        public override void Flush() => inner.Flush();

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);

        /// <inheritdoc/>
        public override void SetLength(long value) => inner.SetLength(value);

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer) => inner.Read(buffer);

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default) => inner.ReadAsync(buffer, ct);

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct) => inner.ReadAsync(buffer, offset, count, ct);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                inner.Dispose();
                owner.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            await inner.DisposeAsync();
            owner.Dispose();
            await base.DisposeAsync();
        }
    }
}