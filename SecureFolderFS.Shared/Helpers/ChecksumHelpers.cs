using System;
using System.Security.Cryptography;
using System.Text;

namespace SecureFolderFS.Shared.Helpers
{
    public static class ChecksumHelpers
    {
        public static string CalculateChecksumForId(string id)
        {
            var buffer = Encoding.UTF8.GetBytes(id);
            var hash = MD5.HashData(buffer);

            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }
    }
}
