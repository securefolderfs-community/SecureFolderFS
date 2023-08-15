using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.Authenticators
{
    /// <inheritdoc cref="IAuthenticator{TAuthentication}"/>
    public sealed class KeyFileAuthenticator : IAuthenticator<IDisposable>
    {
        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        /// <inheritdoc/>
        public async Task<IDisposable> AuthenticateAsync(CancellationToken cancellationToken)
        {
            var keyFile = await FileExplorerService.PickFileAsync(new[] { ".key" }, cancellationToken);
            if (keyFile is null)
                throw new OperationCanceledException("The user did not pick a file.");

            await using var keyStream = await keyFile.OpenStreamAsync(FileAccess.Read, cancellationToken);
            using var secretKey = new SecureKey(128);

            var read = await keyStream.ReadAsync(secretKey.Key, cancellationToken);
            if (read < secretKey.Length)
                throw new DataException("The key data was too short.");

            return secretKey.CreateCopy();
        }
    }
}
