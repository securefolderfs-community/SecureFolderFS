using System;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public interface IRfc3394KeyWrap
    {
        byte[] WrapKey(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> kek);

        byte[] UnwrapKey(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> kek);
    }
}
