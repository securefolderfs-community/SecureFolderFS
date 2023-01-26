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
                    return new CommonResult(new SerializationException("Couldn't deserialize configuration buffer to configuration data model"));

                return new CommonResult(configDataModel.Version == Constants.VaultVersion.LATEST_VERSION);
            }
            catch (Exception ex)
            {
                return new CommonResult(ex);
            }
        }
    }
}