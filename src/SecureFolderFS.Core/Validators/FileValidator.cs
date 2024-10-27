using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Validators
{
    public sealed class FileValidator : IAsyncValidator<IFile>
    {
        private readonly IFolder _vaultFolder;

        public FileValidator(IFolder vaultFolder)
        {
            _vaultFolder = vaultFolder;
        }

        /// <inheritdoc/>
        public Task ValidateAsync(IFile value, CancellationToken cancellationToken = default)
        {
            // TODO: Implement file validation (invalid chunks, invalid name, checksum mismatch, etc...?)
            return Task.CompletedTask;
        }
    }
}
