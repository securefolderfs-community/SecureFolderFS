using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.UI.ViewModels
{
    /// <inheritdoc cref="KeyFileViewModel"/>
    [Bindable(true)]
    public sealed class KeyFileLoginViewModel : KeyFileViewModel
    {
        private readonly IFolder _vaultFolder;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        public KeyFileLoginViewModel(string id, IFolder vaultFolder)
            : base(id)
        {
            _vaultFolder = vaultFolder;
        }

        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var vaultReader = new VaultReader(_vaultFolder, StreamSerializer.Instance);
            var config = await vaultReader.ReadConfigurationAsync(cancellationToken);
            var key = await this.TrySignAsync(config.Uid, null, cancellationToken);
            if (!key.Successful || key.Value is null)
                return;

            CredentialsProvided?.Invoke(this, new(key.Value));
        }
    }
}
