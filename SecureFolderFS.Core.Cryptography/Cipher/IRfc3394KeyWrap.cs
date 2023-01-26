using System;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public interface IRfc3394KeyWrap
    {
        // TODO: Use span here as well
        byte[] WrapKey(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> kek);

        void UnwrapKey(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> kek, Span<byte> result);
    }
}
