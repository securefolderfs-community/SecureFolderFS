using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Cryptography.Cipher.Default
{
    /// <inheritdoc cref="IAesCtrCrypt"/>
    public sealed class AesCtrCrypt : IAesCtrCrypt
    {
        private const ulong CTR_START = 0UL;

        /// <inheritdoc/>
        public void Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, Span<byte> result)
        {
            ulong ulIv = BitConverter.ToUInt64(iv); // TODO: ulIv good here?

            using var aesCtr = new AesCounterMode(ulIv, CTR_START);
            var transformEnc = aesCtr.CreateEncryptor(key.ToArray(), null);

            var result2 = transformEnc.TransformFinalBlock(bytes.ToArray(), 0, bytes.Length);

            result2.CopyTo(result);
        }

        /// <inheritdoc/>
        public bool Decrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, Span<byte> result)
        {
            try
            {
                ulong ulIv = BitConverter.ToUInt64(iv); // TODO: ulIv good here?

                using var aesCtr = new AesCounterMode(ulIv, CTR_START);
                var transformDec = aesCtr.CreateDecryptor(key.ToArray(), null);

                var result2 = transformDec.TransformFinalBlock(bytes.ToArray(), 0, bytes.Length);

                result2.CopyTo(result);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region AES-SIV Implementation

        // The MIT License (MIT)

        // Copyright (c) 2020 Hans Wolff

        // Permission is hereby granted, free of charge, to any person obtaining a copy
        // of this software and associated documentation files (the "Software"), to deal
        // in the Software without restriction, including without limitation the rights
        // to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        // copies of the Software, and to permit persons to whom the Software is
        // furnished to do so, subject to the following conditions:

        // The above copyright notice and this permission notice shall be included in
        // all copies or substantial portions of the Software.

        // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        // FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
        // THE SOFTWARE.

        // Source: https://gist.github.com/hanswolff/8809275

        internal sealed class AesCounterMode : SymmetricAlgorithm
        {
            private readonly ulong _nonce;
            private readonly ulong _counter;
            private readonly Aes _aes;

            public AesCounterMode(ulong nonce, ulong counter)
            {
                _aes = Aes.Create();
                _aes.Mode = CipherMode.ECB;
                _aes.Padding = PaddingMode.None;

                _nonce = nonce;
                _counter = counter;
            }

            public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[]? ignoredParameter)
            {
                return new CounterModeCryptoTransform(_aes, rgbKey, _nonce, _counter);
            }

            public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[]? ignoredParameter)
            {
                return new CounterModeCryptoTransform(_aes, rgbKey, _nonce, _counter);
            }

            public override void GenerateKey()
            {
                _aes.GenerateKey();
            }

            public override void GenerateIV()
            {
                // IV not needed in Counter Mode
            }
        }

        internal sealed class CounterModeCryptoTransform : ICryptoTransform
        {
            private readonly byte[] _nonceAndCounter;
            private readonly ICryptoTransform _counterEncryptor;
            private readonly Queue<byte> _xorMask = new();
            private readonly SymmetricAlgorithm _symmetricAlgorithm;
            private ulong _counter;
            private byte[]? _counterModeBlock;

            public int InputBlockSize => _symmetricAlgorithm.BlockSize / 8;

            public int OutputBlockSize => _symmetricAlgorithm.BlockSize / 8;

            public bool CanTransformMultipleBlocks => true;

            public bool CanReuseTransform => false;

            public CounterModeCryptoTransform(SymmetricAlgorithm symmetricAlgorithm, byte[] key, ulong nonce, ulong counter)
            {
                ArgumentNullException.ThrowIfNull(key);

                _symmetricAlgorithm = symmetricAlgorithm ?? throw new ArgumentNullException(nameof(symmetricAlgorithm));
                _counter = counter;
                _nonceAndCounter = new byte[16];

                BitConverter.TryWriteBytes(_nonceAndCounter, nonce);
                BitConverter.TryWriteBytes(new Span<byte>(_nonceAndCounter, sizeof(ulong), sizeof(ulong)), counter);

                var zeroIv = new byte[_symmetricAlgorithm.BlockSize / 8];
                _counterEncryptor = symmetricAlgorithm.CreateEncryptor(key, zeroIv);
            }

            public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
            {
                var output = new byte[inputCount];
                TransformBlock(inputBuffer, inputOffset, inputCount, output, 0);
                return output;
            }

            public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                for (var i = 0; i < inputCount; i++)
                {
                    if (NeedMoreXorMaskBytes())
                        EncryptCounterThenIncrement();

                    var mask = _xorMask.Dequeue();
                    outputBuffer[outputOffset + i] = (byte)(inputBuffer[inputOffset + i] ^ mask);
                }

                return inputCount;
            }

            private bool NeedMoreXorMaskBytes()
            {
                return _xorMask.Count == 0;
            }

            private void EncryptCounterThenIncrement()
            {
                _counterModeBlock ??= new byte[_symmetricAlgorithm.BlockSize / 8];

                _counterEncryptor.TransformBlock(_nonceAndCounter, 0, _nonceAndCounter.Length, _counterModeBlock, 0);
                IncrementCounter();

                foreach (var b in _counterModeBlock)
                {
                    _xorMask.Enqueue(b);
                }
            }

            private void IncrementCounter()
            {
                _counter++;
                var span = new Span<byte>(_nonceAndCounter, sizeof(ulong), sizeof(ulong));
                BitConverter.TryWriteBytes(span, _counter);
            }

            public void Dispose()
            {
                _counterEncryptor.Dispose();
            }
        }

        #endregion
    }
}
