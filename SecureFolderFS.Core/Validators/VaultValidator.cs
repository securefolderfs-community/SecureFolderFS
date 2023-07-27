using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Utils;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Validators
{
    /// <inheritdoc cref="IAsyncValidator{T}"/>
    internal sealed class VaultValidator : IAsyncValidator<IFolder>
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
            var configFile = await value.GetFileAsync(Constants.Vault.VAULT_CONFIGURATION_FILENAME, cancellationToken);
            await using var configStream = await configFile.OpenStreamAsync(FileAccess.Read, cancellationToken);

            // Validate version
            var versionValidator = new VersionValidator(_serializer);
            await versionValidator.ValidateAsync(configStream, cancellationToken);
        }
    }
}
