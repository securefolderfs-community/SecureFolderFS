using System;
using System.Linq;
using SecureFolderFS.Core.Sdk.SecureStore;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.SecureStore
{
    internal sealed class MasterKey : UnknownStore<MasterKey>
    {
        private readonly SecretKey _encryptionKey;

        private readonly SecretKey _macKey;

        private MasterKey(SecretKey encryptionKey, SecretKey macKey)
        {
            _encryptionKey = encryptionKey;
            _macKey = macKey;
        }

        public bool AnyEmpty()
        {
            return _encryptionKey.Key.IsEmpty() || _macKey.Key.IsEmpty();
        }

        public SecretKey CreateEncryptionKeyCopy()
        {
            return new SecretKey(_encryptionKey.Key.CloneArray());
        }

        public SecretKey CreateMacKeyCopy()
        {
            return new SecretKey(_macKey.Key.CloneArray());
        }

        public override bool Equals(MasterKey? other)
        {
            if (other is null)
            {
                return false;
            }

            return _macKey.Key.SequenceEqual(other._macKey.Key) && _encryptionKey.Key.SequenceEqual(other._encryptionKey.Key);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_encryptionKey, _macKey);
        }

        public static MasterKey Create(SecretKey encryptionKey, SecretKey macKey)
        {
            var encryptionKeyCopy = encryptionKey.CreateCopy();
            var macKeyCopy = macKey.CreateCopy();

            return new MasterKey(encryptionKeyCopy, macKeyCopy);
        }

        protected override void SecureFree()
        {
            _encryptionKey?.Dispose();
            _macKey?.Dispose();
        }
    }
}
