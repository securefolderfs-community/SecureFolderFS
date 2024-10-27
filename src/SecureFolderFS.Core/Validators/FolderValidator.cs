using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Validators
{
    public sealed class FolderValidator : IAsyncValidator<IFolder>
    {
        private readonly IFolder _vaultFolder;

        public FolderValidator(IFolder vaultFolder)
        {
            _vaultFolder = vaultFolder;
        }

        /// <inheritdoc/>
        public async Task ValidateAsync(IFolder value, CancellationToken cancellationToken = default)
        {
            // TODO: Implement folder validation (corrupted/missing directory id, invalid name)
            await Task.Delay(4);
            //return Task.CompletedTask;
        }
    }
}
