using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.SecureStore;
using System;
using System.Security.Cryptography;
using static SecureFolderFS.Core.Cryptography.Constants.Crypto.Headers.AesGcm;
using static SecureFolderFS.Core.Cryptography.Extensions.HeaderCryptExtensions.AesGcmHeaderExtensions;

namespace SecureFolderFS.Core.Cryptography.HeaderCrypt
{
    /// <inheritdoc cref="IHeaderCrypt"/>
    internal sealed class AesGcmHeaderCrypt : BaseHeaderCrypt
    {
        /// <inheritdoc/>
        public override int HeaderCiphertextSize { get; } = HEADER_SIZE;

        /// <inheritdoc/>
        public override int HeaderPlaintextSize { get; } = HEADER_NONCE_SIZE + HEADER_CONTENTKEY_SIZE;

        public AesGcmHeaderCrypt(KeyPair keyPair)
            : base(keyPair)
        {
        }

        /// <inheritdoc/>
        public override void CreateHeader(Span<byte> plaintextHeader)
        {
            // Nonce
            RandomNumberGenerator.Fill(plaintextHeader.Slice(0, HEADER_NONCE_SIZE));

            // Content key
            RandomNumberGenerator.Fill(plaintextHeader.Slice(HEADER_NONCE_SIZE, HEADER_CONTENTKEY_SIZE));
        }

        /// <inheritdoc/>
        public override unsafe void EncryptHeader(ReadOnlySpan<byte> plaintextHeader, Span<byte> ciphertextHeader)
        {
            // Nonce
            plaintextHeader.GetHeaderNonce().CopyTo(ciphertextHeader);

            // Use unsafe pointers to pass span data through the UseKey callback
            fixed (byte* plaintextPtr = plaintextHeader)
            fixed (byte* ciphertextPtr = ciphertextHeader)
            {
                var state = (ptPtr: (nint)plaintextPtr, ptLen: plaintextHeader.Length, ctPtr: (nint)ciphertextPtr, ctLen: ciphertextHeader.Length);
                DekKey.UseKey(state, static (dekKey, s) =>
                {
                    var pt = new ReadOnlySpan<byte>((byte*)s.ptPtr, s.ptLen);
                    var ct = new Span<byte>((byte*)s.ctPtr, s.ctLen);

                    // Encrypt
                    AesGcm128.Encrypt(
                        pt.GetHeaderContentKey(),
                        dekKey,
                        pt.GetHeaderNonce(),
                        ct.GetHeaderTag(),
                        ct.Slice(HEADER_NONCE_SIZE, HEADER_CONTENTKEY_SIZE),
                        ReadOnlySpan<byte>.Empty);
                });
            }
        }

        /// <inheritdoc/>
        public override unsafe bool DecryptHeader(ReadOnlySpan<byte> ciphertextHeader, Span<byte> plaintextHeader)
        {
            // Nonce
            ciphertextHeader.GetHeaderNonce().CopyTo(plaintextHeader);

            // Use unsafe pointers to pass span data through the UseKey callback
            fixed (byte* ciphertextPtr = ciphertextHeader)
            fixed (byte* plaintextPtr = plaintextHeader)
            {
                var state = (ctPtr: (nint)ciphertextPtr, ctLen: ciphertextHeader.Length, ptPtr: (nint)plaintextPtr, ptLen: plaintextHeader.Length);
                return DekKey.UseKey(state, static (dekKey, s) =>
                {
                    var ct = new ReadOnlySpan<byte>((byte*)s.ctPtr, s.ctLen);
                    var pt = new Span<byte>((byte*)s.ptPtr, s.ptLen);

                    // Decrypt
                    return AesGcm128.TryDecrypt(
                        ct.GetHeaderContentKey(),
                        dekKey,
                        ct.GetHeaderNonce(),
                        ct.GetHeaderTag(),
                        pt.Slice(HEADER_NONCE_SIZE),
                        ReadOnlySpan<byte>.Empty);
                });
            }
        }
    }
}
