using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Strategy;

namespace SecureFolderFS.Sdk.AppModels
{
    public sealed class VaultLoginModel : IVaultLoginModel
    {
        private readonly IVaultService _vaultService;
        private readonly IAsyncValidator<IFolder> _vaultValidator;

        /// <inheritdoc/>
        public IVaultModel VaultModel { get; }

        /// <inheritdoc/>
        public IVaultWatcherModel VaultWatcher { get; }

        /// <inheritdoc/>
        public event EventHandler<IVaultStrategyModel>? StrategyChanged;

        public VaultLoginModel(IVaultModel vaultModel, IVaultWatcherModel vaultWatcher)
        {
            VaultModel = vaultModel;
            VaultWatcher = vaultWatcher;
            _vaultService = Ioc.Default.GetRequiredService<IVaultService>();
            _vaultValidator = _vaultService.GetVaultValidator();

            VaultWatcher.VaultChangedEvent += VaultWatcher_VaultChangedEvent;
        }

        private async void VaultWatcher_VaultChangedEvent(object? sender, IResult e)
        {
            if (!e.Successful)
                await DetermineStrategyAsync(default);
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await DetermineStrategyAsync(cancellationToken);
            await VaultWatcher.InitAsync(cancellationToken);
        }

        private async Task DetermineStrategyAsync(CancellationToken cancellationToken)
        {
            var validationResult = await _vaultValidator.ValidateAsync(VaultModel.Folder, cancellationToken);
            // TODO: Use validationResult for 2fa detection as well

            var is2faEnabled = false; // TODO: Just for testing, implement the real code later
            if (is2faEnabled || await VaultModel.Folder.TryGetFileAsync(Core.Constants.VAULT_KEYSTORE_FILENAME, cancellationToken) is not IFile keystoreFile)
            {
                // TODO: Recognize the option in that view model
                StrategyModel = new LoginKeystoreViewModel();
                return;
            }

            if (validationResult.Successful)
            {
                var keystoreModel = new FileKeystoreModel(keystoreFile, StreamSerializer.Instance);
                var vaultUnlockingModel = new VaultUnlockingModel();

                StrategyModel = new LoginCredentialsViewModel(VaultViewModel, vaultUnlockingModel, _vaultWatcherModel, keystoreModel, null); // TODO(r)
            }
            else
                StrategyModel = new LoginErrorViewModel(validationResult.GetMessage("Vault is inaccessible."));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            VaultWatcher.Dispose();
            VaultWatcher.VaultChangedEvent -= VaultWatcher_VaultChangedEvent;
        }
    }
}
