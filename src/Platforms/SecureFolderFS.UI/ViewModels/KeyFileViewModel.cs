using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.ViewModels
{
    /// <inheritdoc cref="AuthenticationViewModel"/>
    public abstract class KeyFileViewModel : AuthenticationViewModel
    {
        private const int KEY_LENGTH = 128;

        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        protected KeyFileViewModel(string id, IFolder vaultFolder)
            : base(id, vaultFolder)
        {
            DisplayName = "Key File";
        }

        /// <inheritdoc/>
        public override Task RevokeAsync(string id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override async Task<IKey> CreateAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            // The 'data' parameter is not needed in this type of authentication
            _ = data;

            using var secureRandom = RandomNumberGenerator.Create();
            using var secretKey = new SecureKey(KEY_LENGTH + id.Length);
            await using var dataStream = new MemoryStream();

            // Fill the first 128 bytes with secure random data
            secureRandom.GetNonZeroBytes(secretKey.Key.AsSpan(0, KEY_LENGTH));

            // Fill the remaining bytes with the ID
            // By using ASCII encoding we get 1:1 byte to char ratio which allows us
            // to use the length of the string ID as part of the SecretKey length above
            Encoding.ASCII.GetBytes(id, secretKey.Key.AsSpan(KEY_LENGTH));

            // Write to the data stream and save the file
            await dataStream.WriteAsync(secretKey.Key, cancellationToken);
            var result = await FileExplorerService.SaveFileAsync("Vault authentication key", dataStream, new Dictionary<string, string>()
            {
                { "Key File", Constants.FileNames.KEY_FILE_EXTENSION },
                { "All Files", "*" }
            }, cancellationToken);

            if (!result)
                throw new OperationCanceledException("The user did not save a file.");

            // Create a copy of the secret key because we need to dispose the original
            return secretKey.CreateCopy();
        }

        /// <inheritdoc/>
        public override async Task<IKey> SignAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            // The 'data' parameter is not needed in this type of authentication
            _ = data;

            var keyFile = await FileExplorerService.PickFileAsync(new[] { ".key", "*" }, cancellationToken);
            if (keyFile is null)
                throw new OperationCanceledException("The user did not pick a file.");

            await using var keyStream = await keyFile.OpenStreamAsync(FileAccess.Read, cancellationToken);
            using var secretKey = new SecureKey(KEY_LENGTH + id.Length);

            var read = await keyStream.ReadAsync(secretKey.Key, cancellationToken);
            if (read < secretKey.Length)
                throw new DataException("The key data was too short.");

            // Create a copy of the secret key because we need to dispose the original
            return secretKey.CreateCopy();
        }
    }
}
