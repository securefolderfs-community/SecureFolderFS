using AVFoundation;
using Foundation;

namespace SecureFolderFS.Maui.Platforms.iOS.AppModels
{
    /// <summary>
    /// Serves AVFoundation resource loading requests directly from a seekable <see cref="Stream"/>.
    /// </summary>
    /// <remarks>
    /// This allows AVFoundation to read media with full random access without materializing
    /// the (decrypted) content in a temporary file. Byte ranges are essential - many videos
    /// keep their metadata ('moov' atom) at the end of the file.
    /// </remarks>
    internal sealed class StreamedResourceLoaderDelegate : AVAssetResourceLoaderDelegate
    {
        // Serve at most this many bytes per request
        private const int MAX_CHUNK_LENGTH = 1024 * 1024;

        private readonly Stream _sourceStream;
        private readonly string _contentTypeUti;
        private readonly object _streamLock = new();

        public StreamedResourceLoaderDelegate(Stream sourceStream, string contentTypeUti)
        {
            _sourceStream = sourceStream;
            _contentTypeUti = contentTypeUti;
        }

        /// <inheritdoc/>
        public override bool ShouldWaitForLoadingOfRequestedResource(AVAssetResourceLoader resourceLoader, AVAssetResourceLoadingRequest loadingRequest)
        {
            try
            {
                var cir = loadingRequest.ContentInformationRequest;
                if (cir is not null)
                {
                    cir.ContentType = _contentTypeUti;
                    cir.ByteRangeAccessSupported = true;
                    lock (_streamLock)
                        cir.ContentLength = _sourceStream.Length;
                }

                if (loadingRequest.DataRequest is { } dataRequest)
                {
                    // CurrentOffset tracks the progress of partially-served requests
                    var offset = dataRequest.CurrentOffset != 0L ? dataRequest.CurrentOffset : dataRequest.RequestedOffset;
                    var length = (int)Math.Min(dataRequest.RequestedLength, MAX_CHUNK_LENGTH);

                    if (length > 0)
                    {
                        int read;
                        var buffer = new byte[length];
                        lock (_streamLock)
                        {
                            _sourceStream.Position = offset;
                            read = _sourceStream.ReadAtLeast(buffer, length, throwOnEndOfStream: false);
                        }

                        if (read > 0)
                        {
                            using var data = NSData.FromArray(read == buffer.Length ? buffer : buffer[..read]);
                            dataRequest.Respond(data);
                        }
                    }
                }

                loadingRequest.FinishLoading();
                return true;
            }
            catch (Exception)
            {
                // The source stream may already be closed when late requests arrive
                loadingRequest.FinishLoadingWithError(new NSError((NSString)$"{nameof(SecureFolderFS)}.ResourceLoader", -1));
                return true;
            }
        }
    }
}
