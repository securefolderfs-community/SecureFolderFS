using System;
using System.Linq;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.SecureStore
{
    internal sealed class MasterKey : FreeableStore<MasterKey> // TODO: Improve security
    {
        private readonly SecretKey _encryptionKey;

        private readonly SecretKey _macKey;

        private MasterKey(SecretKey encryptionKey, SecretKey macKey)
        {
            this._encryptionKey = encryptionKey;
            this._macKey = macKey;
        }

        public bool IsEmpty()
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

        public override MasterKey CreateCopy()
        {
            return Create(_encryptionKey, _macKey);
        }

        public override bool Equals(MasterKey other)
        {
            if (other?._macKey?.Key == null || _macKey?.Key == null || other?._encryptionKey?.Key == null || _encryptionKey?.Key == null)
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
            var encryptionKeyCopy = new SecretKey(encryptionKey.Key.CloneArray());
            var macKeyCopy = new SecretKey(macKey.Key.CloneArray());

            return new MasterKey(encryptionKeyCopy, macKeyCopy);
        }

        protected override void SecureFree()
        {
            _encryptionKey?.Dispose();
            _macKey?.Dispose();
        }
    }
}
