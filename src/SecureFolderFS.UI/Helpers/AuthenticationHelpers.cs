using SecureFolderFS.Core.Cryptography.SecureStore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SecureFolderFS.UI.Helpers
{
    public static class AuthenticationHelpers
    {
        public static SecretKey ParseSecretKey(IEnumerable<IDisposable> passkey)
        {
            var length = 0;
            var data = passkey.Select(x => x switch
            {
                IEnumerable<byte> sequence => sequence.ToArray(),
                _ => null
            }).Where(x =>
            {
                length += x?.Length ?? 0;
                return x is not null;
            }).ToImmutableList(); // Enumerating early is important here since 'length' variable is used before the foreach statement

            var indexInKey = 0;
            var secretKey = new SecureKey(length);
            foreach (var item in data)
            {
                item!.CopyTo(secretKey.Key.AsSpan(indexInKey));
                indexInKey += item!.Length;
            }

            return secretKey;
        }
    }
}
