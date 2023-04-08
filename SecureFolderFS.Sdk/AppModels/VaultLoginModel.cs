using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultLoginModel"/>
    public sealed class VaultLoginModel : IVaultLoginModel
    {
        private readonly IVaultService _vaultService;
        private readonly IAsyncValidator<IFolder> _vaultValidator;

        /// <inheritdoc/>
        public IVaultModel VaultModel { get; }

        /// <inheritdoc/>
        public IVaultWatcherModel VaultWatcher { get; }

        /// <inheritdoc/>
        public event EventHandler<IResult<VaultLoginStateType>>? StateChanged;

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
            // TODO: Use validationResult for 2fa detection as well
            var validationResult = await _vaultValidator.ValidateAsync(VaultModel.Folder, cancellationToken);

            // TODO: 2FA is currently unimplemented
            var is2faEnabled = false;

            if (is2faEnabled)
            {
                // Two-factor authentication
                StateChanged?.Invoke(this, new CommonResult<VaultLoginStateType>(VaultLoginStateType.AwaitingTwoFactorAuth));
            }
            else if (validationResult.Successful)
            {
                // Credentials
                StateChanged?.Invoke(this, new CommonResult<VaultLoginStateType>(VaultLoginStateType.AwaitingCredentials));
            }
            else
            {
                // Vault error
                StateChanged?.Invoke(this, new CommonResult<VaultLoginStateType>(VaultLoginStateType.VaultError, false));
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            VaultWatcher.Dispose();
            VaultWatcher.VaultChangedEvent -= VaultWatcher_VaultChangedEvent;
        }
    }
}
