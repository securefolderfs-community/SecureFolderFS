using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.UI.ViewModels.Authentication
{
    /// <inheritdoc cref="KeyFileViewModel"/>
    [Bindable(true)]
    public sealed class KeyFileCreationViewModel : KeyFileViewModel
    {
        private readonly string _vaultId;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        public KeyFileCreationViewModel(string vaultId)
        {
            _vaultId = vaultId;
        }

        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var key = await CreateAsync(_vaultId, null, cancellationToken);
                CredentialsProvided?.Invoke(this, new(key));
            }
            catch (Exception ex)
            {
                Report(Result.Failure(ex));
            }
        }
    }
}
