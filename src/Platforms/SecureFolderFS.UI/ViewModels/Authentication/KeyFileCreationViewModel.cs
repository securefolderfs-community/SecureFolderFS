using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.Extensions;
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
                var keyResult = await EnrollAsync(_vaultId, null, cancellationToken);
                if (!keyResult.TryGetValue(out var key))
                {
                    Report(keyResult);
                    return;
                }
                
                var tcs = new TaskCompletionSource();
                CredentialsProvided?.Invoke(this, new(key, tcs));
                await tcs.Task;
            }
            catch (Exception ex)
            {
                Report(Result.Failure(ex));
            }
        }
    }
}
