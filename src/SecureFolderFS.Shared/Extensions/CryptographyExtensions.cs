using System;
using System.Security.Cryptography;

namespace SecureFolderFS.Shared.Extensions
{
    public static class CryptographyExtensions
    {
        public static void AppendData(this HMAC hmac, byte[] bytes)
        {
            hmac.TransformBlock(bytes, 0, bytes.Length, null, 0);
        }

        public static void AppendFinalData(this HMAC hmac, byte[] bytes)
        {
            hmac.TransformFinalBlock(bytes, 0, bytes.Length);
        }

        public static void GetCurrentHash(this HMAC hmac, Span<byte> destination)
        {
            hmac.Hash.CopyTo(destination);
        }
    }
}
