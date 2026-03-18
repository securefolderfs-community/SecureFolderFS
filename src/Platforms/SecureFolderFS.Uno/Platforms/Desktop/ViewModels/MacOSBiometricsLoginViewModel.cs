using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Uno.Platforms.Desktop.ViewModels
{
    internal sealed class MacOSBiometricsLoginViewModel : MacOSBiometricsViewModel
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        public MacOSBiometricsLoginViewModel(IFolder vaultFolder, string vaultId)
            : base(vaultFolder, vaultId)
        {
        }

        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var vaultReader = new VaultReader(VaultFolder, StreamSerializer.Instance);
            var auth = await vaultReader.ReadAuthenticationAsync<VaultProtectedKeyDataModel>($"{Id}{Core.Constants.Vault.Names.CONFIGURATION_EXTENSION}", cancellationToken);

            if (auth?.CiphertextKey is null)
            {
                Report(Result.Failure(new ArgumentNullException(nameof(VaultProtectedKeyDataModel.CiphertextKey))));
                return;
            }

            try
            {
                IsAuthenticated = false;
                var keyResult = await AcquireAsync(VaultId, auth.CiphertextKey, cancellationToken);
                IsAuthenticated = true;

                if (!keyResult.TryGetValue(out var keyMaterial))
                {
                    Report(keyResult);
                    return;
                }

                var tcs = new TaskCompletionSource();
                CredentialsProvided?.Invoke(this, new CredentialsProvidedEventArgs(keyMaterial, tcs));
                await tcs.Task;
            }
            catch (Exception ex)
            {
                Report(Result.Failure(ex));
            }
        }
    }
}

