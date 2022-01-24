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
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation.AesCtr
{
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

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] ignoredParameter)
        {
            return new CounterModeCryptoTransform(_aes, rgbKey, _nonce, _counter);
        }

        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] ignoredParameter)
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
}
