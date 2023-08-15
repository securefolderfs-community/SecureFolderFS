using System;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    // TODO: Needs docs
    public static class Rfc3394KeyWrap
    {
        public static byte[] WrapKey(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> kek)
        {
            return RFC3394.KeyWrapAlgorithm.WrapKey(kek: kek.ToArray(), plaintext: bytes.ToArray());
        }

        public static void UnwrapKey(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> kek, Span<byte> result)
        {
            var result2 = RFC3394.KeyWrapAlgorithm.UnwrapKey(kek: kek.ToArray(), ciphertext: bytes.ToArray());
            result2.CopyTo(result);
        }
    }
}
