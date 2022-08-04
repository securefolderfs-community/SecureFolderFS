using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.VaultDataStore;
using SecureFolderFS.Core.VaultDataStore.VaultConfiguration;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.WinUI.AppModels
{
    /// <inheritdoc cref="IAsyncValidator{T}"/>
    internal sealed class VaultValidator : IAsyncValidator<IFolder>
    {
        /// <inheritdoc/>
        public async Task<IResult> ValidateAsync(IFolder value, CancellationToken cancellationToken = default)
        {
            var configFile = await value.GetFileAsync(Core.Constants.VAULT_CONFIGURATION_FILENAME);
            if (configFile is null)
                return new CommonResult(new FileNotFoundException());

            await using var configStream = await configFile.TryOpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken);
            if (configStream is null)
                return new CommonResult(new UnauthorizedAccessException());

            var rawVaultConfiguration = RawVaultConfiguration.Load(configStream);
            if (VaultVersion.IsVersionSupported(rawVaultConfiguration))
            {
                return new CommonResult();
            }

            return new CommonResult(new UnsupportedVaultException(rawVaultConfiguration));
        }
    }
}
