using System.IO;
using SecureFolderFS.Core.Sdk.Streams;

namespace SecureFolderFS.Core.Streams
{
    /// <summary>
    /// Provides implementation for converting SecureFolderFS interface streams.
    /// <br/>
    /// <br/>
    /// This SDK is exposed.
    /// </summary>
    public static class StreamConverters
    {
        public static Stream AsStream(this IBaseFileStream baseFileStream)
        {
            if (baseFileStream is ICleartextFileStream cleartextFileStream)
            {
                return (Stream)((CleartextFileStream)cleartextFileStream);
            }
            else if (baseFileStream is ICiphertextFileStream ciphertextFileStream)
            {
                return (Stream)((CiphertextFileStream)ciphertextFileStream);
            }
            else
            {
                return null;
            }
        }

        public static IBaseFileStream AsBaseFileStream(this Stream stream)
        {
            if (stream is CleartextFileStream cleartextFileStream)
            {
                return (IBaseFileStream)((ICleartextFileStream)cleartextFileStream);
            }
            else if (stream is CiphertextFileStream ciphertextFileStream)
            {
                return (IBaseFileStream)((ICiphertextFileStream)ciphertextFileStream);
            }
            else
            {
                return null;
            }
        }

        public static ICiphertextFileStream AsCiphertextFileStream(this IBaseFileStream baseFileStream)
        {
            if (baseFileStream is CiphertextFileStream ciphertextFileStream)
            {
                return (ICiphertextFileStream)ciphertextFileStream;
            }
            else
            {
                return null;
            }
        }

        public static ICleartextFileStream AsCleartextFileStream(this IBaseFileStream baseFileStream)
        {
            if (baseFileStream is CleartextFileStream cleartextFileStream)
            {
                return (ICleartextFileStream)cleartextFileStream;
            }
            else
            {
                return null;
            }
        }
    }
}
