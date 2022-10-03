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
        private readonly IAsyncSerializer<byte[]> _serializer;

        public VersionValidator(IAsyncSerializer<byte[]> serializer)
        {
            _serializer = serializer;
        }

        /// <inheritdoc/>
        public async Task<IResult> ValidateAsync(Stream value, CancellationToken cancellationToken = default)
        {
            value.Position = 0L;

            var configBuffer = new byte[value.Length];
            var read = await value.ReadAsync(configBuffer, cancellationToken);
            if (read < configBuffer.Length)
                return new CommonResult(new ArgumentException("Reading vault configuration yielded less data than expected."));

            var configDataModel = await _serializer.DeserializeAsync<byte[], VaultConfigurationDataModel?>(configBuffer, cancellationToken);
            if (configDataModel is null)
                return new CommonResult(new SerializationException("Couldn't deserialize config buffer to configuration data model"));

            return new CommonResult(configDataModel.Version == Constants.VaultVersion.LATEST_VERSION);
        }
    }
}
