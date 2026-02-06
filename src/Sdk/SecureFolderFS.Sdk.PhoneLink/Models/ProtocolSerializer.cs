using System;
using System.IO;
using SecureFolderFS.Sdk.PhoneLink.Enums;

namespace SecureFolderFS.Sdk.PhoneLink.Models
{
    public static class ProtocolSerializer
    {
        /// <summary>
        /// Creates a discovery request packet.
        /// </summary>
        public static byte[] CreateDiscoveryRequest(string desktopName)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write(Constants.DISCOVERY_MAGIC);
            writer.Write(Constants.PROTOCOL_VERSION);
            writer.Write((byte)MessageType.DiscoveryRequest);
            writer.Write(desktopName);

            return ms.ToArray();
        }

        public static byte[] CreatePairingRequest(string machineName, byte[] ecdhPublicKey)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.PairingRequest);
            writer.Write(machineName);
            writer.Write(ecdhPublicKey.Length);
            writer.Write(ecdhPublicKey);

            return ms.ToArray();
        }

        public static byte[] CreatePairingConfirmMessage(string credentialId, string vaultName, string pairingId)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.PairingConfirm);
            writer.Write(credentialId);
            writer.Write(vaultName);
            writer.Write(pairingId);

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

        public static byte[] ParsePairingComplete(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);

            reader.ReadByte(); // Skip message type
            var keyLength = reader.ReadInt32();
            return reader.ReadBytes(keyLength);
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
                var magic = reader.ReadBytes(Constants.DISCOVERY_MAGIC.Length);
                if (!magic.SequenceEqual(Constants.DISCOVERY_MAGIC))
                    return null;

                var version = reader.ReadByte();
                if (version != Constants.PROTOCOL_VERSION)
                    return null;

                var messageType = (MessageType)reader.ReadByte();
                if (messageType != MessageType.DiscoveryResponse)
                    return null;

                var deviceId = reader.ReadString();
                var deviceName = reader.ReadString();
                var port = reader.ReadInt32();
                var publicKeyLength = reader.ReadInt32();
                var publicKey = publicKeyLength > 0 ? reader.ReadBytes(publicKeyLength) : null;

                return new DiscoveredDevice
                {
                    DeviceId = deviceId,
                    DeviceName = deviceName,
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
    }
}