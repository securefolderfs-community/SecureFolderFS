using System;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm
{
    public interface IRfc3394KeyWrap
    {
        byte[] Rfc3394WrapKey(byte[] bytes, byte[] kek);

        byte[] Rfc3394UnwrapKey(byte[] bytes, byte[] kek);
    }
}
