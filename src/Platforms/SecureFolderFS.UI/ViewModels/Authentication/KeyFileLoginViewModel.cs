using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.UI.ViewModels.Authentication
{
    /// <inheritdoc cref="KeyFileViewModel"/>
    [Bindable(true)]
    public sealed class KeyFileLoginViewModel(IFolder vaultFolder) : KeyFileViewModel
    {
        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var vaultReader = new VaultReader(vaultFolder, StreamSerializer.Instance);
            var config = await vaultReader.ReadConfigurationAsync(cancellationToken);
            var keyResult = await this.TrySignAsync(config.Uid, null, cancellationToken);
            if (!keyResult.TryGetValue(out var key))
                return;

            var tcs = new TaskCompletionSource();
            CredentialsProvided?.Invoke(this, new(key, tcs));
            await tcs.Task;
        }
    }
}
