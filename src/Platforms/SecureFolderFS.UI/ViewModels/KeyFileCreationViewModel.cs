using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.EventArguments;

namespace SecureFolderFS.UI.ViewModels
{
    public sealed partial class KeyFileCreationViewModel : KeyFileViewModel
    {
        private readonly string _vaultId;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        public KeyFileCreationViewModel(string vaultId, string id, IFolder vaultFolder)
            : base(id, vaultFolder)
        {
            _vaultId = vaultId;
        }

        [RelayCommand]
        private async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var key = await CreateAsync(_vaultId, null, cancellationToken);
            CredentialsProvided?.Invoke(this, new(key));
        }
    }
}
