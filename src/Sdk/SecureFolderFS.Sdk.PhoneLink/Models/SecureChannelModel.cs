using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using static SecureFolderFS.Sdk.PhoneLink.Constants;

namespace SecureFolderFS.Sdk.PhoneLink.Models
{
    /// <summary>
    /// Provides an AES-256-GCM encrypted communication channel with replay protection.
    /// Used after secure pairing is established.
    /// </summary>
    public sealed class SecureChannelModel : IDisposable
    {
        /// <summary>
        /// Size of the sliding window for replay protection (in bits).
        /// Allows messages to arrive up to 64 positions out of order.
        /// </summary>
        private const int REPLAY_WINDOW_SIZE = 64;

        private readonly byte[] _encryptionKey;
        private readonly object _replayLock = new();
        private ulong _replayWindow;  // Bitmap for tracking received sequences within window
        private uint _highestReceivedSeq;  // Highest sequence number received
        private int _sendSequence;
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
        [SkipLocalsInit]
        public byte[] Encrypt(byte[] plaintext)
        {
            ThrowIfDisposed();

            using var aes = new AesGcm(_encryptionKey, KeyTraits.TAG_SIZE);

            // Build nonce: 4 bytes sequence + 8 bytes random
            Span<byte> nonce = stackalloc byte[KeyTraits.NONCE_SIZE];
            var sequence = (uint)Interlocked.Increment(ref _sendSequence);
            BitConverter.TryWriteBytes(nonce.Slice(0, 4), sequence);
            RandomNumberGenerator.Fill(nonce.Slice(4));

            var ciphertext = new byte[plaintext.Length];
            Span<byte> tag = stackalloc byte[KeyTraits.TAG_SIZE];

            aes.Encrypt(nonce, plaintext, ciphertext, tag);

            var output = new byte[KeyTraits.NONCE_SIZE + ciphertext.Length + KeyTraits.TAG_SIZE];
            nonce.CopyTo(output);
            ciphertext.CopyTo(output, KeyTraits.NONCE_SIZE);
            tag.CopyTo(output.AsSpan(KeyTraits.NONCE_SIZE + ciphertext.Length));

            return output;
        }

        /// <summary>
        /// Decrypts a message with replay protection.
        /// </summary>
        /// <exception cref="CryptographicException">Thrown if decryption fails or replay attack detected.</exception>
        public byte[] Decrypt(ReadOnlySpan<byte> encryptedData)
        {
            ThrowIfDisposed();

            if (encryptedData.Length < KeyTraits.NONCE_SIZE + KeyTraits.TAG_SIZE)
                throw new CryptographicException("Invalid encrypted data length");

            // Extract sequence number from nonce for replay check
            var sequence = BitConverter.ToUInt32(encryptedData.Slice(0, 4));

            // Check for replay attack before decryption (fail fast)
            if (!TryAcceptSequence(sequence))
                throw new CryptographicException("Replay attack detected: duplicate or outdated sequence number");

            using var aes = new AesGcm(_encryptionKey, KeyTraits.TAG_SIZE);
            var nonce = encryptedData.Slice(0, KeyTraits.NONCE_SIZE);
            var ciphertextLength = encryptedData.Length - KeyTraits.NONCE_SIZE - KeyTraits.TAG_SIZE;
            var ciphertext = encryptedData.Slice(KeyTraits.NONCE_SIZE, ciphertextLength);
            var tag = encryptedData.Slice(KeyTraits.NONCE_SIZE + ciphertextLength, KeyTraits.TAG_SIZE);

            var plaintext = new byte[ciphertextLength];
            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            return plaintext;
        }

        /// <summary>
        /// Attempts to accept a sequence number using sliding window algorithm.
        /// </summary>
        /// <param name="sequence">The sequence number to check.</param>
        /// <returns>True if the sequence is valid and not a replay; false otherwise.</returns>
        private bool TryAcceptSequence(uint sequence)
        {
            lock (_replayLock)
            {
                // First message ever received
                if (_highestReceivedSeq == 0 && _replayWindow == 0)
                {
                    _highestReceivedSeq = sequence;
                    _replayWindow = 1;  // Mark position 0 as received
                    return true;
                }

                if (sequence > _highestReceivedSeq)
                {
                    // New highest sequence - shift window
                    var shift = sequence - _highestReceivedSeq;
                    if (shift >= REPLAY_WINDOW_SIZE)
                    {
                        // Completely new window
                        _replayWindow = 1;
                    }
                    else
                    {
                        // Shift and mark new position
                        _replayWindow <<= (int)shift;
                        _replayWindow |= 1;
                    }
                    _highestReceivedSeq = sequence;
                    return true;
                }

                // Sequence is within or before window
                var diff = _highestReceivedSeq - sequence;
                if (diff >= REPLAY_WINDOW_SIZE)
                {
                    // Too old - outside window
                    return false;
                }

                // Check if already received (bit is set)
                var bit = 1UL << (int)diff;
                if ((_replayWindow & bit) != 0)
                {
                    // Already received - replay detected
                    return false;
                }

                // Mark as received
                _replayWindow |= bit;
                return true;
            }
        }

        /// <summary>
        /// Computes 6-digit verification code from shared secret.
        /// </summary>
        [SkipLocalsInit]
        public static string ComputeVerificationCode(ReadOnlySpan<byte> sharedSecret)
        {
            Span<byte> hash = stackalloc byte[4];
            HKDF.DeriveKey(
                HashAlgorithmName.SHA256,
                sharedSecret,
                hash,
                ReadOnlySpan<byte>.Empty,
                "PhoneLink-VerificationCode-v1"u8.ToArray());

            var code = BitConverter.ToUInt32(hash) % 1000000;
            return code.ToString("D6");
        }

        /// <summary>
        /// Performs ECDH key exchange and derives shared secret.
        /// </summary>
        public static byte[] DeriveSharedSecret(ECDiffieHellman localPrivateKey, ReadOnlySpan<byte> remotePublicKey)
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

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            CryptographicOperations.ZeroMemory(_encryptionKey);
        }
    }
}