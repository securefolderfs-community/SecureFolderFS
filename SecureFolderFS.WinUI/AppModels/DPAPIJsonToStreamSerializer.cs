using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.WinUI.AppModels
{
    /// <summary>
    /// Uses <see cref="Stream"/> to serialize/deserialize JSON with DPAPI encryption layer.
    /// </summary>
    internal sealed class DPAPIJsonToStreamSerializer : JsonToStreamSerializer
    {
        /// <inheritdoc/>
        public override Task<Stream> SerializeAsync<TData>(TData data, CancellationToken cancellationToken = default)
        {
            var serialized = JsonConvert.SerializeObject(data, typeof(TData), Formatting.Indented, null);
            var buffer = EncryptDataFromString(serialized);

            return Task.FromResult<Stream>(new MemoryStream(buffer));
        }

        /// <inheritdoc/>
        public override async Task<TData?> DeserializeAsync<TData>(Stream serialized, CancellationToken cancellationToken = default) where TData : default
        {
            var buffer = await DecryptDataFromStreamAsync(serialized, DataProtectionScope.CurrentUser).ConfigureAwait(false);
            var rawSerialized = Encoding.UTF8.GetString(buffer);

            return JsonConvert.DeserializeObject<TData?>(rawSerialized);
        }

        private static async Task<byte[]> DecryptDataFromStreamAsync(Stream stream, DataProtectionScope scope)
        {
            var encryptedBuffer = await stream.ReadAllBytesAsync().ConfigureAwait(false);

            if (encryptedBuffer.IsEmpty())
                return encryptedBuffer;

            return ProtectedData.Unprotect(encryptedBuffer, null, scope);
        }

        private static byte[] EncryptDataFromString(string data)
        {
            var buffer = Encoding.UTF8.GetBytes(data);
            return ProtectedData.Protect(buffer, null, DataProtectionScope.CurrentUser);
        }
    }
}
