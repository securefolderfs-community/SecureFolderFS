using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Dialogs
{
    [Inject<IVaultService>, Inject<IPasswordChangeService>]
    public sealed partial class PasswordChangeDialogViewModel : DialogViewModel, IDisposable
    {
        private readonly IVaultModel _vaultModel;

        [ObservableProperty] private bool _IsInvalidPasswordShown;

        public PasswordChangeDialogViewModel(IVaultModel vaultModel)
        {
            ServiceProvider = Ioc.Default;
            _vaultModel = vaultModel;
        }

        public async Task<bool> ChangePasswordAsync(PasswordPair? passwordPair, CancellationToken cancellationToken)
        {
            if (passwordPair is null)
                return false;

            var keystoreResult = await _vaultModel.Folder.GetFileWithResultAsync(VaultService.KeystoreFileName, cancellationToken);
            if (!keystoreResult.Successful || keystoreResult.Value is null)
                return false; // TODO: Notify

            using var keystoreModel = new FileKeystoreModel(keystoreResult.Value, StreamSerializer.Instance);
            var result = await PasswordChangeService.SetVaultFolderAsync(_vaultModel.Folder, cancellationToken);
            if (!result.Successful)
                return false; // TODO: Notify

            result = await PasswordChangeService.SetKeystoreAsync(keystoreModel, cancellationToken);
            if (!result.Successful)
                return false; // TODO: Notify

            result = await PasswordChangeService.ChangePasswordAsync(passwordPair.ExistingPassword, passwordPair.NewPassword, cancellationToken);
            if (!result.Successful)
            {
                IsInvalidPasswordShown = true;
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            PasswordChangeService.Dispose();
        }
    }

    public sealed record PasswordPair(IPassword ExistingPassword, IPassword NewPassword);
}
