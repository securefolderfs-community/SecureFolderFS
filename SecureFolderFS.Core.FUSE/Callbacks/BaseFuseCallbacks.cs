using System.Text;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FUSE.OpenHandles;
using Tmds.Fuse;

namespace SecureFolderFS.Core.FUSE.Callbacks
{
    internal abstract class BaseFuseCallbacks : FuseFileSystemBase
    {
        protected readonly IPathConverter pathConverter;
        protected readonly HandlesManager handlesManager;

        protected BaseFuseCallbacks(IPathConverter pathConverter, HandlesManager handlesManager)
        {
            this.pathConverter = pathConverter;
            this.handlesManager = handlesManager;
        }

        protected abstract string? GetCiphertextPath(ReadOnlySpan<byte> cleartextName);

        /// <returns>
        /// A pointer to a byte array containing UTF-8-encoded ciphertext path, or null if the path couldn't be
        /// obtained.
        /// </returns>
        protected unsafe byte* GetCiphertextPathPointer(ReadOnlySpan<byte> cleartextName)
        {
            var cipherTextPath = GetCiphertextPath(cleartextName);
            return cipherTextPath == null ? null : ToUtf8ByteArray(cipherTextPath);
        }

        protected unsafe byte* ToUtf8ByteArray(string str)
        {
            fixed (byte *ptr = Encoding.UTF8.GetBytes(str))
                return ptr;
        }
    }
}