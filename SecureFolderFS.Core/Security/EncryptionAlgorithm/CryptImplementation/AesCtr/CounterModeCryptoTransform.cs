
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

using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation.AesCtr
{
    internal sealed class CounterModeCryptoTransform : ICryptoTransform
    {
        private readonly byte[] _nonceAndCounter;

        private readonly ICryptoTransform _counterEncryptor;

        private readonly Queue<byte> _xorMask = new();

        private readonly SymmetricAlgorithm _symmetricAlgorithm;

        private ulong _counter;

        private byte[] _counterModeBlock;

        public int InputBlockSize => _symmetricAlgorithm.BlockSize / 8;

        public int OutputBlockSize => _symmetricAlgorithm.BlockSize / 8;

        public bool CanTransformMultipleBlocks => true;

        public bool CanReuseTransform => false;

        public CounterModeCryptoTransform(SymmetricAlgorithm symmetricAlgorithm, byte[] key, ulong nonce, ulong counter)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

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

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer,
            int outputOffset)
        {
            for (var i = 0; i < inputCount; i++)
            {
                if (NeedMoreXorMaskBytes())
                {
                    EncryptCounterThenIncrement();
                }

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
}
