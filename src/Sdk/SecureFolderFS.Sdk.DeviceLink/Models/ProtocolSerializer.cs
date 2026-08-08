using System;
using System.IO;
using SecureFolderFS.Sdk.DeviceLink.Enums;
using static SecureFolderFS.Sdk.DeviceLink.Constants;

namespace SecureFolderFS.Sdk.DeviceLink.Models
{
    public static class ProtocolSerializer
    {
        #region Discovery

        /// <summary>
        /// Creates a discovery request packet.
        /// </summary>
        public static byte[] CreateDiscoveryRequest(string desktopName)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write(DISCOVERY_MAGIC);
            writer.Write(PROTOCOL_VERSION);
            writer.Write((byte)MessageType.DiscoveryRequest);
            writer.Write(desktopName);

            return ms.ToArray();
        }

        /// <summary>
        /// Validates a discovery request packet.
        /// </summary>
        public static bool ValidateDiscoveryRequest(byte[] buffer)
        {
            if (buffer.Length < DISCOVERY_MAGIC.Length + 2)
                return false;

            var magic = buffer.AsSpan(0, DISCOVERY_MAGIC.Length).ToArray();
            return magic.SequenceEqual(DISCOVERY_MAGIC);
        }

        /// <summary>
        /// Creates a discovery response packet.
        /// </summary>
        public static byte[] CreateDiscoveryResponse(string deviceId, string deviceName, string deviceType, int communicationPort)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write(DISCOVERY_MAGIC);
            writer.Write(PROTOCOL_VERSION);
            writer.Write((byte)MessageType.DiscoveryResponse);
            writer.Write(deviceId);
            writer.Write(deviceName);
            writer.Write(deviceType);
            writer.Write(communicationPort);

            // Include empty public key placeholder (actual signing key comes after pairing)
            writer.Write(0);

            return ms.ToArray();
        }

        /// <summary>
        /// Parses a discovery response packet.
        /// </summary>
        public static DiscoveredDevice? ParseDiscoveryResponse(byte[] data, string senderIp)
        {
            try
            {
                using var ms = new MemoryStream(data);
                using var reader = new BinaryReader(ms);

                // Verify magic bytes
                var magic = reader.ReadBytes(DISCOVERY_MAGIC.Length);
                if (!magic.SequenceEqual(DISCOVERY_MAGIC))
                    return null;

                var version = reader.ReadByte();
                if (version != PROTOCOL_VERSION)
                    return null;

                var messageType = (MessageType)reader.ReadByte();
                if (messageType != MessageType.DiscoveryResponse)
                    return null;

                var deviceId = reader.ReadString();
                var deviceName = reader.ReadString();
                var deviceType = reader.ReadString();
                var port = reader.ReadInt32();
                var publicKeyLength = reader.ReadInt32();
                var publicKey = publicKeyLength > 0 ? reader.ReadBytes(publicKeyLength) : null;

                return new DiscoveredDevice
                {
                    DeviceId = deviceId,
                    DeviceName = deviceName,
                    DeviceType = deviceType,
                    IpAddress = senderIp,
                    Port = port,
                    PublicKey = publicKey
                };
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Pairing

        /// <summary>
        /// Creates a pairing request carrying both ECDH and ML-KEM public keys.
        /// The ML-KEM public key allows the mobile side to encapsulate a shared secret
        /// that only the desktop can decapsulate (quantum-resistant key agreement).
        /// </summary>
        public static byte[] CreatePairingRequest(
            string desktopName,
            string desktopType,
            byte[] ecdhPublicKey,
            byte[] mlKemPublicKey)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.PairingRequest);
            writer.Write(desktopName);
            writer.Write(desktopType);
            writer.Write(ecdhPublicKey.Length);
            writer.Write(ecdhPublicKey);
            writer.Write(mlKemPublicKey.Length);
            writer.Write(mlKemPublicKey);

            return ms.ToArray();
        }

        /// <summary>
        /// Parses a pairing request, extracting both ECDH and ML-KEM public keys.
        /// </summary>
        public static void ParsePairingRequest(
            byte[] message,
            out string desktopName,
            out string desktopType,
            out byte[] ecdhPublicKey,
            out byte[] mlKemPublicKey)
        {
            using var ms = new MemoryStream(message);
            using var reader = new BinaryReader(ms);

            reader.ReadByte(); // Skip message type
            desktopName = reader.ReadString();
            desktopType = reader.ReadString();

            var ecdhKeyLength = reader.ReadInt32();
            ecdhPublicKey = reader.ReadBytes(ecdhKeyLength);

            var mlKemKeyLength = reader.ReadInt32();
            mlKemPublicKey = reader.ReadBytes(mlKemKeyLength);
        }

        /// <summary>
        /// Creates a pairing response carrying the mobile's ECDH public key and the
        /// ML-KEM ciphertext (encapsulated against the desktop's ML-KEM public key).
        /// </summary>
        public static byte[] CreatePairingResponse(byte[] ecdhPublicKey, byte[] mlKemCiphertext)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.PairingResponse);
            writer.Write(ecdhPublicKey.Length);
            writer.Write(ecdhPublicKey);
            writer.Write(mlKemCiphertext.Length);
            writer.Write(mlKemCiphertext);

            return ms.ToArray();
        }

        /// <summary>
        /// Parses a pairing response, extracting the mobile's ECDH public key and ML-KEM ciphertext.
        /// </summary>
        public static void ParsePairingResponse(
            byte[] data,
            out byte[] ecdhPublicKey,
            out byte[] mlKemCiphertext)
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);

            reader.ReadByte(); // Skip message type

            var ecdhKeyLength = reader.ReadInt32();
            ecdhPublicKey = reader.ReadBytes(ecdhKeyLength);

            var ciphertextLength = reader.ReadInt32();
            mlKemCiphertext = reader.ReadBytes(ciphertextLength);
        }

        public static byte[] CreatePairingConfirmMessage(string credentialId, string vaultName, string pairingId, byte[] challenge)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.PairingConfirm);
            writer.Write(credentialId);
            writer.Write(vaultName);
            writer.Write(pairingId);
            writer.Write(challenge.Length);
            writer.Write(challenge);

            return ms.ToArray();
        }

        public static void ParsePairingConfirm(byte[] message, out string credentialId, out string vaultName, out string pairingId, out byte[] challenge)
        {
            using var ms = new MemoryStream(message);
            using var reader = new BinaryReader(ms);

            reader.ReadByte(); // Skip message type
            credentialId = reader.ReadString();
            vaultName = reader.ReadString();
            pairingId = reader.ReadString();

            var challengeLength = reader.ReadInt32();
            challenge = reader.ReadBytes(challengeLength);
        }

        /// <summary>
        /// Parses a pairing complete message, extracting the vault key contribution and the
        /// channel binding secret (two independent, domain-separated derivations of the mobile's HMAC key).
        /// </summary>
        public static void ParsePairingComplete(byte[] data, out byte[] keyContribution, out byte[] bindingSecret)
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);

            reader.ReadByte(); // Skip message type
            var keyLength = reader.ReadInt32();
            keyContribution = reader.ReadBytes(keyLength);
            var bindingLength = reader.ReadInt32();
            bindingSecret = reader.ReadBytes(bindingLength);
        }

        /// <summary>
        /// Creates a pairing complete message carrying the vault key contribution and the
        /// channel binding secret (encrypted before transmission).
        /// </summary>
        public static byte[] CreatePairingComplete(ReadOnlySpan<byte> keyContribution, ReadOnlySpan<byte> bindingSecret)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.PairingComplete);
            writer.Write(keyContribution.Length);
            writer.Write(keyContribution);
            writer.Write(bindingSecret.Length);
            writer.Write(bindingSecret);

            return ms.ToArray();
        }

        #endregion

        #region Secure Session

        /// <summary>
        /// Creates a secure session request carrying both ECDH and ML-KEM public keys.
        /// </summary>
        public static byte[] CreateSecureSessionRequest(
            string pairingId,
            ReadOnlySpan<byte> nonce,
            ReadOnlySpan<byte> ecdhPublicKey,
            ReadOnlySpan<byte> mlKemPublicKey)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.SecureSessionRequest);
            writer.Write(pairingId);
            writer.Write(nonce.Length);
            writer.Write(nonce);
            writer.Write(ecdhPublicKey.Length);
            writer.Write(ecdhPublicKey);
            writer.Write(mlKemPublicKey.Length);
            writer.Write(mlKemPublicKey);

            return ms.ToArray();
        }

        /// <summary>
        /// Parses a secure session request, extracting the nonce, ECDH public key, and ML-KEM public key.
        /// </summary>
        public static void ParseSecureSessionRequest(
            byte[] message,
            out string pairingId,
            out byte[] nonce,
            out byte[] ecdhPublicKey,
            out byte[] mlKemPublicKey)
        {
            using var ms = new MemoryStream(message);
            using var reader = new BinaryReader(ms);

            reader.ReadByte(); // Skip message type
            pairingId = reader.ReadString();

            var nonceLength = reader.ReadInt32();
            nonce = reader.ReadBytes(nonceLength);

            var ecdhKeyLength = reader.ReadInt32();
            ecdhPublicKey = reader.ReadBytes(ecdhKeyLength);

            var mlKemKeyLength = reader.ReadInt32();
            mlKemPublicKey = reader.ReadBytes(mlKemKeyLength);
        }

        /// <summary>
        /// Creates a secure session accepted response carrying the mobile's ECDH public key
        /// and the ML-KEM ciphertext encapsulated against the desktop's ML-KEM public key.
        /// </summary>
        public static byte[] CreateSecureSessionAccepted(
            ReadOnlySpan<byte> nonce,
            ReadOnlySpan<byte> ecdhPublicKey,
            ReadOnlySpan<byte> mlKemCiphertext)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.SecureSessionAccepted);
            writer.Write(nonce.Length);
            writer.Write(nonce);
            writer.Write(ecdhPublicKey.Length);
            writer.Write(ecdhPublicKey);
            writer.Write(mlKemCiphertext.Length);
            writer.Write(mlKemCiphertext);

            return ms.ToArray();
        }

        /// <summary>
        /// Parses a secure session accepted response, extracting the nonce, ECDH public key,
        /// and ML-KEM ciphertext.
        /// </summary>
        public static void ParseSecureSessionAccepted(
            byte[] data,
            out byte[] nonce,
            out byte[] ecdhPublicKey,
            out byte[] mlKemCiphertext)
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);

            reader.ReadByte(); // Skip message type

            var nonceLength = reader.ReadInt32();
            nonce = reader.ReadBytes(nonceLength);

            var ecdhKeyLength = reader.ReadInt32();
            ecdhPublicKey = reader.ReadBytes(ecdhKeyLength);

            var ciphertextLength = reader.ReadInt32();
            mlKemCiphertext = reader.ReadBytes(ciphertextLength);
        }

        #endregion

        #region Authentication

        /// <summary>
        /// Creates an authentication request. Carries the persistent challenge alongside a fresh
        /// per-request nonce; the mobile must echo the nonce in its response, proving the response
        /// was produced for this exact request rather than replayed.
        /// </summary>
        public static byte[] CreateSecureAuthRequest(string credentialId, ReadOnlySpan<byte> challenge, long timestamp, ReadOnlySpan<byte> requestNonce)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write(credentialId);
            writer.Write(challenge.Length);
            writer.Write(challenge);
            writer.Write(timestamp);
            writer.Write(requestNonce.Length);
            writer.Write(requestNonce);

            return ms.ToArray();
        }

        /// <summary>
        /// Parses an authentication request (already decrypted from the secure channel).
        /// </summary>
        public static void ParseSecureAuthRequest(byte[] data, out string credentialId, out byte[] challenge, out long timestamp, out byte[] requestNonce)
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);

            credentialId = reader.ReadString();
            var challengeLength = reader.ReadInt32();
            challenge = reader.ReadBytes(challengeLength);
            timestamp = reader.ReadInt64();
            var nonceLength = reader.ReadInt32();
            requestNonce = reader.ReadBytes(nonceLength);
        }

        /// <summary>
        /// Creates an authentication response (encrypted before transmission). Carries the stable
        /// vault key contribution together with the echoed request nonce.
        /// </summary>
        public static byte[] CreateSecureAuthResponse(ReadOnlySpan<byte> keyContribution, ReadOnlySpan<byte> echoedNonce)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write(keyContribution.Length);
            writer.Write(keyContribution);
            writer.Write(echoedNonce.Length);
            writer.Write(echoedNonce);

            return ms.ToArray();
        }

        /// <summary>
        /// Parses an authentication response (already decrypted from the secure channel).
        /// </summary>
        public static void ParseSecureAuthResponse(byte[] data, out byte[] keyContribution, out byte[] echoedNonce)
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);

            var keyLength = reader.ReadInt32();
            keyContribution = reader.ReadBytes(keyLength);
            var nonceLength = reader.ReadInt32();
            echoedNonce = reader.ReadBytes(nonceLength);
        }

        public static byte[] CreateAuthenticationRejected(string reason)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.AuthenticationRejected);
            writer.Write(reason);

            return ms.ToArray();
        }

        #endregion
    }
}