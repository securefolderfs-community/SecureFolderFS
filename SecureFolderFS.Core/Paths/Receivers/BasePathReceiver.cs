using System;
using SecureFolderFS.Sdk.Paths;

namespace SecureFolderFS.Core.Paths.Receivers
{
    internal abstract class BasePathReceiver : IPathReceiver
    {
        protected readonly VaultPath vaultPath;

        protected BasePathReceiver(VaultPath vaultPath)
        {
            this.vaultPath = vaultPath;
        }

        public TRequestedPath FromCiphertextPath<TRequestedPath>(string ciphertextPath) where TRequestedPath : IPath
        {
            var requestedPathType = typeof(TRequestedPath);

            if (typeof(ICleartextPath).IsAssignableFrom(requestedPathType))
            {
                return (TRequestedPath)CleartextPathFromRawCiphertextPath(ciphertextPath);
            }
            else if (typeof(ICiphertextPath).IsAssignableFrom(requestedPathType))
            {
                return (TRequestedPath)CiphertextPathFromRawCiphertextPath(ciphertextPath);
            }
            else
            {
                throw new ArgumentException($"Could not assign {requestedPathType.Name} from ciphertext path.");
            }
        }

        public TRequestedPath FromCleartextPath<TRequestedPath>(string cleartextPath) where TRequestedPath : IPath
        {
            var requestedPathType = typeof(TRequestedPath);

            if (typeof(ICleartextPath).IsAssignableFrom(requestedPathType))
            {
                return (TRequestedPath)CleartextPathFromRawCleartextPath(cleartextPath);
            }
            else if (typeof(ICiphertextPath).IsAssignableFrom(requestedPathType))
            {
                return (TRequestedPath)CiphertextPathFromRawCleartextPath(cleartextPath);
            }
            else
            {
                throw new ArgumentException($"Could not assign {requestedPathType.Name} from cleartext path.");
            }
        }

        public virtual ICleartextPath FromCiphertextPath(ICiphertextPath ciphertextPath)
        {
            return CleartextPathFromRawCiphertextPath(ciphertextPath.Path);
        }

        public virtual ICiphertextPath FromCleartextPath(ICleartextPath cleartextPath)
        {
            return CiphertextPathFromRawCleartextPath(cleartextPath.Path);
        }

        protected virtual ICleartextPath CleartextPathFromRawCleartextPath(string cleartextPath)
        {
            return new CleartextPath(cleartextPath);
        }

        protected virtual ICiphertextPath CiphertextPathFromRawCiphertextPath(string ciphertextPath)
        {
            return new CiphertextPath(ciphertextPath);
        }

        public abstract string GetCleartextFileName(string cleartextFilePath);

        public abstract string GetCiphertextFileName(string ciphertextFilePath);

        protected abstract ICleartextPath CleartextPathFromRawCiphertextPath(string ciphertextPath);

        protected abstract ICiphertextPath CiphertextPathFromRawCleartextPath(string cleartextPath);

        public abstract void Dispose();
    }
}
