// Some parts of the following code were used from Secomba/Base4K on the MIT License basis.
// See the associated license file for more information.

using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public enum Base4KVersion
    {
        V1,
        V2
    }

    public static class SecombaBase4K
    {
        // Base addresses for mapping regions
        private const int BASE_FLAG_START = 0x04000;
        private const int BASE1_START = 0x06000;
        private const int BASE1_START_LEGACY = 0x05000;

        // Sizes of each mapping region
        private const int BASE_FLAG_SIZE = 0x100;
        private const int BASE1_SIZE = 0x01000;

        private static readonly UTF8Encoding Utf8Encoding = new UTF8Encoding(true, true);

        /// <summary>
        /// Encodes the specified raw bytes as a Base4K string, mapping each group of bits
        /// to Unicode characters in a specific range, suitable for use as file names.
        /// </summary>
        /// <param name="raw">The raw bytes to encode.</param>
        /// <param name="version">The version of Base4K encoding to use. Defaults to <see cref="Base4KVersion.V2"/>.</param>
        /// <returns>A Base4K-encoded string representation of the input bytes.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="raw"/> is empty or too short to encode.</exception>
        [SkipLocalsInit]
        public static string Encode(ReadOnlySpan<byte> raw, Base4KVersion version = Base4KVersion.V2)
        {
            if (raw.Length <= 1)
                throw new ArgumentException("Input must be at least 2 bytes long.", nameof(raw));

            var maxByteCount = (raw.Length + 1) * 3;
            var rentedBuffer = ArrayPool<byte>.Shared.Rent(maxByteCount);
            try
            {
                var buffer = rentedBuffer.AsSpan();
                var bufferPos = 0;
                Span<byte> utf8Buffer = stackalloc byte[4];
                int offset;

                for (var i = 0; i < raw.Length * 2 - 2; i += 3)
                {
                    offset = i % 2 == 0
                        ? ((raw[i / 2] << 4) | ((raw[i / 2 + 1] >> 4) & 0x0f)) & 0x0fff
                        : ((raw[i / 2] << 8) | (raw[i / 2 + 1] & 0xff)) & 0x0fff;

                    offset += version == Base4KVersion.V1 ? BASE1_START_LEGACY : BASE1_START;

                    var written = ToUtf8(offset, utf8Buffer);
                    utf8Buffer.Slice(0, written).CopyTo(buffer.Slice(bufferPos));
                    bufferPos += written;
                }

                if ((raw.Length * 2) % 3 == 2)
                {
                    offset = (raw[^1] & 0xff) + BASE_FLAG_START;
                    var written = ToUtf8(offset, utf8Buffer);
                    utf8Buffer.Slice(0, written).CopyTo(buffer.Slice(bufferPos));
                    bufferPos += written;
                }
                else if ((raw.Length * 2) % 3 == 1)
                {
                    offset = (raw[^1] & 0x0f) + BASE_FLAG_START;
                    var written = ToUtf8(offset, utf8Buffer);
                    utf8Buffer.Slice(0, written).CopyTo(buffer.Slice(bufferPos));
                    bufferPos += written;
                }

                return Utf8Encoding.GetString(buffer.Slice(0, bufferPos));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentedBuffer);
            }
        }

        /// <summary>
        /// Decodes a Base4K-encoded string back to the original raw bytes.
        /// Attempts decoding with both V2 and V1 (legacy) base addresses automatically.
        /// </summary>
        /// <param name="encoded">The Base4K-encoded string to decode.</param>
        /// <returns>The decoded bytes, or <see langword="null"/> if decoding failed due to invalid or malformed input.</returns>
        public static byte[]? Decode(ReadOnlySpan<char> encoded)
        {
            return DecodeInternal(encoded, BASE1_START) ?? DecodeInternal(encoded, BASE1_START_LEGACY);
        }

        private static byte[]? DecodeInternal(ReadOnlySpan<char> encoded, int base1Start)
        {
            var byteCount = Utf8Encoding.GetByteCount(encoded);
            var encBytes = new byte[byteCount];
            var written = Utf8Encoding.GetBytes(encoded, encBytes);

            using var memoryStream = new MemoryStream();
            var rentedCollector = ArrayPool<int>.Shared.Rent(written / 3 + 1);
            var collectorCount = 0;
            try
            {
                for (var i = 0; i < written;)
                {
                    int nrOfBytes;
                    if ((encBytes[i] & 0x80) == 0)
                    {
                        // 1 byte
                        nrOfBytes = 1;
                    }
                    else if ((encBytes[i] & 0x40) == 0)
                    {
                        // Continuation byte — invalid as a leading byte
                        return null;
                    }
                    else if ((encBytes[i] & 0x20) == 0)
                    {
                        // 2 bytes
                        nrOfBytes = 2;
                    }
                    else if ((encBytes[i] & 0x10) == 0)
                    {
                        // 3 bytes
                        nrOfBytes = 3;
                    }
                    else if ((encBytes[i] & 0x08) == 0)
                    {
                        // 4 bytes
                        nrOfBytes = 4;
                    }
                    else
                    {
                        // Invalid leading byte
                        return null;
                    }

                    var code = ToCode(encBytes, i, nrOfBytes);
                    i += nrOfBytes;

                    if (!(code >= base1Start && code < base1Start + BASE1_SIZE))
                    {
                        if (i < written || !(code >= BASE_FLAG_START && code < BASE_FLAG_START + BASE_FLAG_SIZE))
                            return null;
                    }

                    rentedCollector[collectorCount++] = code;
                }

                for (var i = 0; i < collectorCount; i++)
                {
                    if (rentedCollector[i] >= base1Start)
                        rentedCollector[i] -= base1Start;
                    else
                    {
                        rentedCollector[i] -= BASE_FLAG_START;
                        if (i % 2 == 0)
                            memoryStream.WriteByte((byte)rentedCollector[i]);
                        else
                            memoryStream.WriteByte((byte)(((rentedCollector[i - 1] << 4) | ((rentedCollector[i] & 0x0f)) & 0xff)));

                        break;
                    }

                    if (i % 2 == 0)
                        memoryStream.WriteByte((byte)(rentedCollector[i] >> 4));
                    else
                    {
                        memoryStream.WriteByte((byte)(((rentedCollector[i - 1] << 4) | ((rentedCollector[i] & 0x0f00) >> 8)) & 0xff));
                        memoryStream.WriteByte((byte)(rentedCollector[i] & 0xff));
                    }
                }
            }
            finally
            {
                ArrayPool<int>.Shared.Return(rentedCollector);
            }

            return memoryStream.ToArray();
        }

        private static int ToUtf8(int code, Span<byte> destination)
        {
            switch (code)
            {
                case > 0xffff:
                {
                    destination[0] = (byte)(0xf0 | ((code >> 18) & 0x07));
                    destination[1] = (byte)(0x80 | ((code >> 12) & 0x3f));
                    destination[2] = (byte)(0x80 | ((code >> 6) & 0x3f));
                    destination[3] = (byte)(0x80 | (code & 0x3f));
                    return 4;
                }

                case > 0x7ff:
                {
                    destination[0] = (byte)(0xe0 | ((code >> 12) & 0x0f));
                    destination[1] = (byte)(0x80 | ((code >> 6) & 0x3f));
                    destination[2] = (byte)(0x80 | (code & 0x3f));
                    return 3;
                }

                case > 0x7f:
                {
                    destination[0] = (byte)(0xc0 | ((code >> 6) & 0x1f));
                    destination[1] = (byte)(0x80 | (code & 0x3f));
                    return 2;
                }

                default:
                {
                    destination[0] = (byte)(code & 0x7f);
                    return 1;
                }
            }
        }

        private static int ToCode(ReadOnlySpan<byte> utf8Char, int offset, int length)
        {
            var result = 0;
            switch (length)
            {
                case 1:
                {
                    result |= utf8Char[offset];
                    break;
                }

                case 2:
                {
                    result |= (utf8Char[offset + 0] & 0x1f) << 6;
                    result |= (utf8Char[offset + 1] & 0x3f);
                    break;
                }

                case 3:
                {
                    result |= (utf8Char[offset + 0] & 0x0f) << 12;
                    result |= (utf8Char[offset + 1] & 0x3f) << 6;
                    result |= (utf8Char[offset + 2] & 0x3f);
                    break;
                }

                case 4:
                {
                    result |= (utf8Char[offset + 0] & 0x07) << 18;
                    result |= (utf8Char[offset + 1] & 0x3f) << 12;
                    result |= (utf8Char[offset + 2] & 0x3f) << 6;
                    result |= (utf8Char[offset + 3] & 0x3f);
                    break;
                }
            }

            return result;
        }
    }
}