using System;
using System.Security.Cryptography;
using System.Text;

namespace SecureFolderFS.Shared.Helpers
{
    public static class EncodingHelpers
    {
        /// <summary>
        /// Gets a hashed version of the <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The ID of an storable object.</param>
        /// <returns>A MD5-hashed string of the ID.</returns>
        public static string EncodeStorableId(string id)
        {
            var buffer = Encoding.UTF8.GetBytes(id);
            var hash = MD5.HashData(buffer);

            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        public static string EncodeBaseUrl64(string base64str)
        {
            var trimmedStr = base64str.TrimEnd('=');
            return trimmedStr.Replace('/', '_').Replace('+', '-');
        }

        public static string DecodeBaseUrl64(string base64UrlStr)
        {
            var decoded = base64UrlStr.Replace('_', '/').Replace('-', '+');
            return (decoded.Length % 4) switch
            {
                2 => decoded + "==",
                3 => decoded + "=",
                _ => decoded
            };
        }
    }
}
