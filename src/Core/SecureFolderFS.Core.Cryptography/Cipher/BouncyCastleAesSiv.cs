using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    /// <summary>
    /// A pure-managed AES-CMAC-SIV (RFC 5297) implementation built on BouncyCastle.
    /// </summary>
    internal static class BouncyCastleAesSiv
    {
        private const int BlockSize = 16;

        /// <summary>
        /// Seals <paramref name="plaintext"/> with a single associated-data item, returning
        /// SIV(16) || ciphertext.
        /// </summary>
        public static byte[] Seal(ReadOnlySpan<byte> key, ReadOnlySpan<byte> associatedData, ReadOnlySpan<byte> plaintext)
        {
            SplitKey(key, out var macKey, out var ctrKey);

            var message = plaintext.ToArray();
            var v = S2V(macKey, associatedData, message);

            var output = new byte[BlockSize + message.Length];
            v.CopyTo(output.AsSpan(0, BlockSize));
            Ctr(ctrKey, v, message, 0, message.Length, output, BlockSize);
            return output;
        }

        /// <summary>
        /// Opens SIV(16) || ciphertext, returning the plaintext or throwing on an integrity failure.
        /// </summary>
        public static byte[] Open(ReadOnlySpan<byte> key, ReadOnlySpan<byte> associatedData, ReadOnlySpan<byte> input)
        {
            if (input.Length < BlockSize)
                throw new CryptographicException("Malformed or corrupt ciphertext.");

            SplitKey(key, out var macKey, out var ctrKey);

            var v = input.Slice(0, BlockSize).ToArray();
            var ciphertext = input.Slice(BlockSize);

            var plaintext = new byte[ciphertext.Length];
            Ctr(ctrKey, v, ciphertext.ToArray(), 0, ciphertext.Length, plaintext, 0);

            var expected = S2V(macKey, associatedData, plaintext);
            if (!CryptographicOperations.FixedTimeEquals(expected, v))
                throw new CryptographicException("Malformed or corrupt ciphertext.");

            return plaintext;
        }

        private static void SplitKey(ReadOnlySpan<byte> key, out byte[] macKey, out byte[] ctrKey)
        {
            if (key.Length != 32 && key.Length != 64)
                throw new CryptographicException("Specified key is not a valid size for this algorithm.");

            var half = key.Length / 2;
            macKey = key.Slice(0, half).ToArray();
            ctrKey = key.Slice(half).ToArray();
        }

        /// <summary>RFC 5297 S2V over a single header string and the message.</summary>
        private static byte[] S2V(byte[] macKey, ReadOnlySpan<byte> header, byte[] message)
        {
            var d = Cmac(macKey, new byte[BlockSize]);

            // Single associated-data item
            Dbl(d);
            Xor(d, Cmac(macKey, header.ToArray()), BlockSize);

            if (message.Length >= BlockSize)
            {
                // T = message with its last block XORed into D
                var t = (byte[])message.Clone();
                var offset = t.Length - BlockSize;
                for (var i = 0; i < BlockSize; i++)
                    t[offset + i] ^= d[i];

                return Cmac(macKey, t);
            }

            var padded = new byte[BlockSize];
            message.CopyTo(padded, 0);
            padded[message.Length] = 0x80; // pad

            Dbl(d);
            Xor(d, padded, BlockSize);
            return Cmac(macKey, d);
        }

        private static byte[] Cmac(byte[] key, byte[] data)
        {
            var mac = new CMac(new AesEngine());
            mac.Init(new KeyParameter(key));
            mac.BlockUpdate(data, 0, data.Length);
            var result = new byte[mac.GetMacSize()];
            mac.DoFinal(result, 0);
            return result;
        }

        private static void Ctr(byte[] key, byte[] siv, byte[] input, int inputOffset, int length, byte[] output, int outputOffset)
        {
            // Zero out the two bits that RFC 5297 reserves so the counter never wraps into them.
            var counter = (byte[])siv.Clone();
            counter[counter.Length - 8] &= 0x7F;
            counter[counter.Length - 4] &= 0x7F;

            // Manual CTR mode in which the full 128-bit counter is incremented (big-endian) and its AES
            // encryption is XORed into the data (the partial final block is handled by only consuming as many keystream bytes as remain).
            var engine = new AesEngine();
            engine.Init(true, new KeyParameter(key));

            var keystream = new byte[BlockSize];
            for (var position = 0; position < length; position += BlockSize)
            {
                engine.ProcessBlock(counter, 0, keystream, 0);

                var count = Math.Min(BlockSize, length - position);
                for (var i = 0; i < count; i++)
                    output[outputOffset + position + i] = (byte)(input[inputOffset + position + i] ^ keystream[i]);

                IncrementBigEndian(counter);
            }
        }

        private static void IncrementBigEndian(byte[] counter)
        {
            for (var i = counter.Length - 1; i >= 0; i--)
            {
                if (++counter[i] != 0)
                    break;
            }
        }

        /// <summary>Doubles a 128-bit value in GF(2^128) (the "dbl" operation).</summary>
        private static void Dbl(byte[] block)
        {
            var carry = block[0] >> 7;
            for (var i = 0; i < BlockSize - 1; i++)
                block[i] = (byte)((block[i] << 1) | (block[i + 1] >> 7));

            block[BlockSize - 1] = (byte)((block[BlockSize - 1] << 1) ^ (carry == 1 ? 0x87 : 0x00));
        }

        private static void Xor(byte[] destination, byte[] source, int length)
        {
            for (var i = 0; i < length; i++)
                destination[i] ^= source[i];
        }
    }
}
