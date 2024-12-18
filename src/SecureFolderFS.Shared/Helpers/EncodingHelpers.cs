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
    }
}
