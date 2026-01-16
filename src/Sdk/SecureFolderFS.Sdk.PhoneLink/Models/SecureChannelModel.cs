using System;
using System.Security.Cryptography;
using System.Threading;
using static SecureFolderFS.Sdk.PhoneLink.Constants;

namespace SecureFolderFS.Sdk.PhoneLink.Models
{
    /// <summary>
    /// Provides an AES-256-GCM encrypted communication channel.
    /// Used after secure pairing is established.
    /// </summary>
    public sealed class SecureChannelModel : IDisposable
    {
        private readonly byte[] _encryptionKey;
        private long _sendSequence;
        private long _receiveSequence;
        private bool _disposed;

        /// <summary>
        /// Creates a secure channel from a shared secret.
        /// </summary>
        public SecureChannelModel(byte[] sharedSecret, byte[]? salt = null)
        {
            salt ??= [];

            _encryptionKey = HKDF.DeriveKey(
                HashAlgorithmName.SHA256,
                sharedSecret,
                32,
                salt,
                "PhoneLink-Encryption-v1"u8.ToArray());
        }

        /// <summary>
        /// Encrypts a message using AES-256-GCM.
        /// </summary>
        public byte[] Encrypt(byte[] plaintext)
        {
            ThrowIfDisposed();

            using var aes = new AesGcm(_encryptionKey, KeyTraits.TAG_SIZE);

            var nonce = new byte[KeyTraits.NONCE_SIZE];
            var seqBytes = BitConverter.GetBytes(Interlocked.Increment(ref _sendSequence));
            Array.Copy(seqBytes, nonce, Math.Min(seqBytes.Length, 4));
            RandomNumberGenerator.Fill(nonce.AsSpan(4));

            var ciphertext = new byte[plaintext.Length];
            var tag = new byte[KeyTraits.TAG_SIZE];

            aes.Encrypt(nonce, plaintext, ciphertext, tag);

            var output = new byte[KeyTraits.NONCE_SIZE + ciphertext.Length + KeyTraits.TAG_SIZE];
            nonce.CopyTo(output, 0);
            ciphertext.CopyTo(output, KeyTraits.NONCE_SIZE);
            tag.CopyTo(output, KeyTraits.NONCE_SIZE + ciphertext.Length);

            return output;
        }

        /// <summary>
        /// Decrypts a message.
        /// </summary>
        public byte[] Decrypt(byte[] encryptedData)
        {
            ThrowIfDisposed();

            if (encryptedData.Length < KeyTraits.NONCE_SIZE + KeyTraits.TAG_SIZE)
                throw new CryptographicException("Invalid encrypted data length");

            using var aes = new AesGcm(_encryptionKey, KeyTraits.TAG_SIZE);

            var nonce = encryptedData.AsSpan(0, KeyTraits.NONCE_SIZE);
            var ciphertextLength = encryptedData.Length - KeyTraits.NONCE_SIZE - KeyTraits.TAG_SIZE;
            var ciphertext = encryptedData.AsSpan(KeyTraits.NONCE_SIZE, ciphertextLength);
            var tag = encryptedData.AsSpan(KeyTraits.NONCE_SIZE + ciphertextLength, KeyTraits.TAG_SIZE);

            var plaintext = new byte[ciphertextLength];
            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            Interlocked.Increment(ref _receiveSequence);

            return plaintext;
        }

        /// <summary>
        /// Computes 6-digit verification code from shared secret.
        /// </summary>
        public static string ComputeVerificationCode(byte[] sharedSecret)
        {
            var hash = HKDF.DeriveKey(
                HashAlgorithmName.SHA256,
                sharedSecret,
                4,
                [],
                "PhoneLink-VerificationCode-v1"u8.ToArray());

            var code = BitConverter.ToUInt32(hash) % 1000000;
            return code.ToString("D6");
        }

        /// <summary>
        /// Performs ECDH key exchange and derives shared secret.
        /// </summary>
        public static byte[] DeriveSharedSecret(ECDiffieHellman localPrivateKey, byte[] remotePublicKey)
        {
            using var remoteKey = ECDiffieHellman.Create();
            remoteKey.ImportSubjectPublicKeyInfo(remotePublicKey, out _);
            return localPrivateKey.DeriveKeyMaterial(remoteKey.PublicKey);
        }

        /// <summary>
        /// Generates a new ECDH key pair.
        /// </summary>
        public static ECDiffieHellman GenerateKeyPair()
        {
            return ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            CryptographicOperations.ZeroMemory(_encryptionKey);
        }
    }
}