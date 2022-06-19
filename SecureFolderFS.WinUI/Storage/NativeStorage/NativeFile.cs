using System;
using System.IO;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Enums;

namespace SecureFolderFS.WinUI.Storage.NativeStorage
{
    /// <inheritdoc cref="IFile"/>
    internal sealed class NativeFile : NativeBaseStorage, IFile
    {
        /// <inheritdoc/>
        public string Extension { get; }

        public NativeFile(string path)
            : base(path)
        {
            Extension = System.IO.Path.GetExtension(path);
        }

        /// <inheritdoc/>
        public Task<Stream?> OpenStreamAsync(FileAccess access)
        {
            return OpenStreamAsync(access, FileShare.None);
        }

        /// <inheritdoc/>
        public Task<Stream?> OpenStreamAsync(FileAccess access, FileShare share)
        {
            try
            {
                var stream = File.Open(Path, FileMode.Open, access, share);
                return Task.FromResult<Stream?>(stream);
            }
            catch (FileNotFoundException)
            {
                return Task.FromResult<Stream?>(null);
            }
        }

        /// <inheritdoc/>
        public Task<Stream> GetThumbnailStreamAsync(uint requestedSize)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override Task<bool> RenameAsync(string newName, NameCollisionOption options)
        {
            if (string.IsNullOrEmpty(newName))
                return Task.FromResult(false);

            var parentPath = System.IO.Path.GetDirectoryName(Path);
            if (string.IsNullOrEmpty(parentPath))
                return Task.FromResult(false);

            try
            {
                var newPath = System.IO.Path.Combine(parentPath, newName);
                File.Move(Path, newPath);

                Path = newPath;
                Name = newName;

                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <inheritdoc/>
        public override Task<bool> DeleteAsync(bool permanently)
        {
            _ = permanently; // TODO: Use this parameter

            try
            {
                File.Delete(Path);
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }
    }
}
