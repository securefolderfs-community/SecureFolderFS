using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using static SecureFolderFS.Core.Constants.Vault.Versions;

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
        public async Task ValidateAsync(Stream value, CancellationToken cancellationToken = default)
        {
            var versionDataModel = await _serializer.DeserializeAsync<Stream, VersionDataModel?>(value, cancellationToken);
            if (versionDataModel is null)
                throw new SerializationException($"Couldn't deserialize configuration buffer to {nameof(VersionDataModel)}.");

            if (versionDataModel.Version > LATEST_VERSION)
                throw new FormatException("Unknown vault version.");

            if (versionDataModel.Version < V1)
                throw new FormatException("Invalid vault version.");

            _ = versionDataModel.Version switch
            {
                // (V1 or Vn) except LATEST_VERSION are not supported
                (V1 or V1) and not LATEST_VERSION =>
                    throw new NotSupportedException($"Vault version {versionDataModel.Version} is not supported.") { Data = { { "Version", versionDataModel.Version } } },

                // More cases...

                // Default
                _ => 0
            };
        }
    }
}