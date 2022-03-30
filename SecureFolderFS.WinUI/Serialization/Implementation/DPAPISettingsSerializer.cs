using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

#nullable enable

namespace SecureFolderFS.WinUI.Serialization.Implementation
{
    internal sealed class DPAPISettingsSerializer : DefaultSettingsSerializer
    {
        public override string? ReadFromFile()
        {
            ArgumentNullException.ThrowIfNull(filePath);

            using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var decrypted = DecryptDataFromStream(fileStream, DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(decrypted);
        }

        public override bool WriteToFile(string? text)
        {
            ArgumentNullException.ThrowIfNull(filePath);

            using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);

            var buffer = Encoding.UTF8.GetBytes(text ?? string.Empty);

            _ = EncryptDataToStream(fileStream, buffer, DataProtectionScope.CurrentUser);

            return true;
        }

        private static byte[] DecryptDataFromStream(Stream stream, DataProtectionScope scope)
        {
            var readBuffer = new byte[stream.Length];

            var readLength = stream.Read(readBuffer, 0, readBuffer.Length);

            if (readLength != stream.Length) throw new IOException("Read smaller than stream size");
            if (readLength == 0) return readBuffer; // Return empty

            return ProtectedData.Unprotect(readBuffer, null, scope);
        }

        private static long EncryptDataToStream(Stream stream, byte[] buffer, DataProtectionScope scope)
        {
            var encryptedBuffer = ProtectedData.Protect(buffer, null, scope);

            stream.Write(encryptedBuffer, 0, encryptedBuffer.Length);
            return stream.Length;
        }
    }
}
