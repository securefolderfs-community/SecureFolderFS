using SecureFolderFS.Core.Routines;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Utils;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.AppModels
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
            var configFileResult = await value.GetFileWithResultAsync(Core.Constants.VAULT_CONFIGURATION_FILENAME, cancellationToken);
            if (!configFileResult.Successful)
                return configFileResult;

            var configStreamResult = await configFileResult.Value!.OpenStreamWithResultAsync(FileAccess.Read, FileShare.Read, cancellationToken);
            if (!configStreamResult.Successful)
                return configStreamResult;

            await using (configStreamResult.Value)
            {
                var versionValidator = VaultRoutines.NewVersionValidator(_serializer);
                return await versionValidator.ValidateAsync(configStreamResult.Value!, cancellationToken);
            }
        }
    }
}
