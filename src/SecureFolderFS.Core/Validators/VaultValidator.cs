using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Validators
{
    /// <inheritdoc cref="IAsyncValidator{T}"/>
    public sealed class VaultValidator : IAsyncValidator<IFolder>
    {
        private readonly IAsyncSerializer<Stream> _serializer;

        public VaultValidator(IAsyncSerializer<Stream> serializer)
        {
            _serializer = serializer;
        }

        /// <inheritdoc/>
        public async Task ValidateAsync(IFolder value, CancellationToken cancellationToken = default)
        {
            // Get configuration file
            if (await value.GetFirstByNameAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, cancellationToken) is not IFile configFile)
                throw new InvalidOperationException("The provided name does not point to a file.");

            // Validate version
            await using var configStream = await configFile.OpenStreamAsync(FileAccess.Read, cancellationToken);
            var versionValidator = new VersionValidator(_serializer);
            await versionValidator.ValidateAsync(configStream, cancellationToken);
        }
    }
}
