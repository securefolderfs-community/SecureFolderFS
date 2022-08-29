namespace SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation
{
    internal sealed class Rfc3394KeyWrap : IRfc3394KeyWrap
    {
        public byte[] Rfc3394WrapKey(byte[] bytes, byte[] kek)
        {
            return RFC3394.KeyWrapAlgorithm.WrapKey(kek: kek, plaintext: bytes);
        }

        public byte[] Rfc3394UnwrapKey(byte[] bytes, byte[] kek)
        {
            return RFC3394.KeyWrapAlgorithm.UnwrapKey(kek: kek, ciphertext: bytes);
        }
    }
}
