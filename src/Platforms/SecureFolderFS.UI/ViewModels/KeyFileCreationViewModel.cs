using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.EventArguments;

namespace SecureFolderFS.UI.ViewModels
{
    /// <inheritdoc cref="KeyFileViewModel"/>
    [Bindable(true)]
    public sealed class KeyFileCreationViewModel : KeyFileViewModel
    {
        private readonly string _vaultId;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        public KeyFileCreationViewModel(string vaultId, string id)
            : base(id)
        {
            _vaultId = vaultId;
        }

        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var key = await CreateAsync(_vaultId, null, cancellationToken);
            CredentialsProvided?.Invoke(this, new(key));
        }
    }
}
