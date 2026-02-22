using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Core.Cryptography.Helpers;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Uno.ViewModels.YubiKey
{
    /// <inheritdoc cref="YubiKeyViewModel"/>
    [Bindable(true)]
    public sealed class YubiKeyCreationViewModel(IFolder vaultFolder, string vaultId)
        : YubiKeyViewModel(vaultFolder, vaultId)
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var vaultWriter = new VaultWriter(VaultFolder, StreamSerializer.Instance);
            using var challenge = CryptHelpers.GenerateChallenge(VaultId, Core.Cryptography.Constants.KeyTraits.CHALLENGE_KEY_PART_LENGTH_64);

            // Write authentication data to the vault, including the slot preference
            await vaultWriter.WriteAuthenticationAsync<VaultChallengeDataModel>(
                $"{Id}{Constants.Vault.Names.CONFIGURATION_EXTENSION}", new()
                {
                    Capability = "supportsChallenge|" + (UseLongPress ? "useLongPress" : "useShortPress"),
                    Challenge = challenge.Key,
                }, cancellationToken);

            try
            {
                var keyResult = await EnrollAsync(VaultId, challenge.Key, cancellationToken);
                if (!keyResult.TryGetValue(out var key))
                {
                    Report(keyResult);
                    return;
                }
                
                // Report that credentials were provided
                var tcs = new TaskCompletionSource();
                CredentialsProvided?.Invoke(this, new(key, tcs));

                await tcs.Task;
            }
            catch (Exception ex)
            {
                // Thrown when authentication fails or YubiKey is not present
                Report(Result.Failure(ex));
            }
        }
    }
}
