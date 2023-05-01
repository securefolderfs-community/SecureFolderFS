using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Validators
{
    /// <inheritdoc cref="IAsyncValidator{T}"/>
    internal sealed class VersionValidator : IAsyncValidator<Stream>
    {
        private readonly IAsyncSerializer<Stream> _serializer;

        public VersionValidator(IAsyncSerializer<Stream> serializer)
        {
            _serializer = serializer;
        }

        /// <inheritdoc/>
        public async Task<IResult> ValidateAsync(Stream value, CancellationToken cancellationToken = default)
        {
            try
            {
                var configDataModel = await _serializer.DeserializeAsync<Stream, VaultConfigurationDataModel?>(value, cancellationToken);
                if (configDataModel is null)
                    return new CommonResult<VaultConfigurationDataModel>(new SerializationException("Couldn't deserialize configuration buffer to configuration data model"));

                if (configDataModel.Version != Constants.VaultVersion.LATEST_VERSION)
                    return new CommonResult<VaultConfigurationDataModel>(new NotSupportedException($"Vault version {configDataModel.Version} is not supported."));

                return new CommonResult<VaultConfigurationDataModel>(configDataModel);
            }
            catch (Exception ex)
            {
                return new CommonResult<VaultConfigurationDataModel>(ex);
            }
        }
    }
}