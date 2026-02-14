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

        public static byte[] CreatePairingRequest(string machineName, string machineType, byte[] ecdhPublicKey)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.PairingRequest);
            writer.Write(machineName);
            writer.Write(machineType);
            writer.Write(ecdhPublicKey.Length);
            writer.Write(ecdhPublicKey);

            return ms.ToArray();
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

        public static byte[] ParsePairingResponse(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);

            reader.ReadByte(); // Skip message type
            var keyLength = reader.ReadInt32();
            return reader.ReadBytes(keyLength);
        }

        public static byte[] CreatePairingResponse(byte[] ecdhPublicKey)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.PairingResponse);
            writer.Write(ecdhPublicKey.Length);
            writer.Write(ecdhPublicKey);

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

        public static byte[] ParsePairingComplete(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);

            reader.ReadByte(); // Skip message type
            var hmacLength = reader.ReadInt32();
            return reader.ReadBytes(hmacLength);
        }

        public static byte[] CreatePairingComplete(ReadOnlySpan<byte> hmacResult)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.PairingComplete);
            writer.Write(hmacResult.Length);
            writer.Write(hmacResult);

            return ms.ToArray();
        }

        #endregion

        #region Secure Session

        public static byte[] CreateSecureSessionRequest(string pairingId, ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> ecdhPublicKey)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.SecureSessionRequest);
            writer.Write(pairingId);
            writer.Write(nonce.Length);
            writer.Write(nonce);
            writer.Write(ecdhPublicKey.Length);
            writer.Write(ecdhPublicKey);

            return ms.ToArray();
        }

        public static void ParseSecureSessionAccepted(byte[] data, out byte[] nonce, out byte[] ecdhPublicKey)
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);

            reader.ReadByte(); // Skip message type

            var nonceLength = reader.ReadInt32();
            nonce = reader.ReadBytes(nonceLength);

            var keyLength = reader.ReadInt32();
            ecdhPublicKey = reader.ReadBytes(keyLength);
        }

        public static byte[] CreateSecureSessionAccepted(ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> ecdhPublicKey)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.SecureSessionAccepted);
            writer.Write(nonce.Length);
            writer.Write(nonce);
            writer.Write(ecdhPublicKey.Length);
            writer.Write(ecdhPublicKey);

            return ms.ToArray();
        }

        #endregion

        #region Authentication

        /// <summary>
        /// Creates auth request for challenge-sign model.
        /// Sends the challenge for mobile to sign.
        /// </summary>
        public static byte[] CreateSecureAuthRequest(string credentialId, ReadOnlySpan<byte> challenge, long timestamp)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write(credentialId);
            writer.Write(challenge.Length);
            writer.Write(challenge);
            writer.Write(timestamp);

            return ms.ToArray();
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