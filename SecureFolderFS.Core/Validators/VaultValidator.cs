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
        public async Task<IResult> ValidateAsync(IFolder value, CancellationToken cancellationToken = default)
        {
            var contentFolderResult = await value.GetFolderWithResultAsync(Constants.CONTENT_FOLDERNAME, cancellationToken);
            if (!contentFolderResult.Successful)
                return contentFolderResult;

            var configFileResult = await value.GetFileWithResultAsync(Constants.VAULT_CONFIGURATION_FILENAME, cancellationToken);
            if (!configFileResult.Successful)
                return configFileResult;

            var configStreamResult = await configFileResult.Value!.OpenStreamWithResultAsync(FileAccess.Read, FileShare.Read, cancellationToken);
            if (!configStreamResult.Successful)
                return configStreamResult;

            await using (configStreamResult.Value)
            {
                var versionValidator = new VersionValidator(_serializer);
                return await versionValidator.ValidateAsync(configStreamResult.Value!, cancellationToken);
            }
        }
    }
}
