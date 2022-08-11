using System;
using System.Security.Cryptography;
using System.Text;

namespace SecureFolderFS.WinUI.Helpers
{
    internal static class ChecksumHelpers
    {
        public static string CalculateChecksumForPath(string path)
        {
            using var md5 = MD5.Create();

            var buffer = Encoding.UTF8.GetBytes(path);
            var hash = md5.ComputeHash(buffer);

            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }
    }
}
