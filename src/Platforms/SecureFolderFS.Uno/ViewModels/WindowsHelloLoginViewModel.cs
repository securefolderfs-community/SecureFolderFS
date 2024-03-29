using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.Helpers;
using Windows.Security.Credentials;

namespace SecureFolderFS.Uno.ViewModels
{
    public sealed partial class WindowsHelloLoginViewModel(string id, IFolder vaultFolder)
        : WindowsHelloViewModel(id, vaultFolder)
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        [RelayCommand]
        private async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var vaultReader = new VaultReader(VaultFolder, StreamSerializer.Instance);
            var config = await vaultReader.ReadConfigurationAsync(cancellationToken);
            var auth = await vaultReader.ReadAuthenticationAsync(cancellationToken);

            if (auth?.Challenge is null)
            {
                SetError(Result.Failure(new ArgumentNullException(nameof(auth))));
                return;
            }

            try
            {
                // Ask for credentials
                var result = await KeyCredentialManager.OpenAsync(config.Uid).AsTask(cancellationToken);
                if (result.Status != KeyCredentialStatus.Success)
                {
                    SetError(Result.Failure(new InvalidOperationException("Failed to open the credential.")));
                    return;
                }

                // Generate key signature based on the user credentials
                var key = await CreateSignatureAsync(result.Credential, auth.Challenge, cancellationToken);

                // Compile new challenge in preparation
                // TODO: When doing the signing operation, check payload MAC
                // TODO: Do something to avoid triggering the Windows Hello dialog twice
                //using var newChallenge = GenerateChallenge(config.Id);
                //using var newSignedChallenge = await CreateSignatureAsync(result.Credential, newChallenge.Key, cancellationToken);

                // Report that credentials were provided and new provision needs to be applied
                CredentialsProvided?.Invoke(this, new CredentialsProvidedEventArgs(key));
                //StateChanged?.Invoke(this, new CredentialsProvisionChangedEventArgs(newChallenge.CreateCopy(), newSignedChallenge.CreateCopy()));
            }
            catch (InvalidOperationException iopex)
            {
                // Thrown when authentication is canceled
                SetError(Result.Failure(iopex));
            }
        }
    }
}
