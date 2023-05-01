using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Strategy;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    public sealed partial class VaultLoginPageViewModel : BaseVaultPageViewModel, IRecipient<VaultUnlockedMessage>
    {
        private readonly IVaultLoginModel _vaultLoginModel;

        [ObservableProperty] private string? _VaultName;
        [ObservableProperty] private INotifyPropertyChanged? _StrategyViewModel;

        private IThreadingService ThreadingService { get; } = Ioc.Default.GetRequiredService<IThreadingService>();

        public VaultLoginPageViewModel(VaultViewModel vaultViewModel, INavigationService navigationService)
            : base(vaultViewModel, navigationService)
        {
            VaultName = vaultViewModel.VaultModel.VaultName;
            _vaultLoginModel = new VaultLoginModel(vaultViewModel.VaultModel, new VaultWatcherModel(vaultViewModel.VaultModel.Folder));
            _vaultLoginModel.StateChanged += VaultLoginModel_StateChanged;

            WeakReferenceMessenger.Default.Register(this);
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await _vaultLoginModel.InitAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void Receive(VaultUnlockedMessage message)
        {
            // Free resources that are no longer being used for login
            if (VaultViewModel.VaultModel.Equals(message.VaultModel))
                Dispose();
        }

        private async void VaultLoginModel_StateChanged(object? sender, IResult<VaultLoginStateType> e)
        {
            // Dispose existing strategy
            (StrategyViewModel as IDisposable)?.Dispose();

            // Switch thread
            await ThreadingService.ChangeThreadAsync();

            // Choose new strategy
            switch (e.Value)
            {
                case VaultLoginStateType.AwaitingCredentials:
                    if (e is ResultWithKeystore resultWithKeystore)
                        StrategyViewModel = new LoginCredentialsViewModel(VaultViewModel, resultWithKeystore.Keystore, _vaultLoginModel.VaultWatcher, new VaultUnlockingModel(), NavigationService);
                    break;

                case VaultLoginStateType.AwaitingTwoFactorAuth:
                    StrategyViewModel = new LoginKeystoreViewModel();
                    break;

                default:
                case VaultLoginStateType.VaultError:
                    StrategyViewModel = new LoginErrorViewModel(e.GetMessage());
                    break;
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _vaultLoginModel.Dispose();
            _vaultLoginModel.StateChanged -= VaultLoginModel_StateChanged;
            (StrategyViewModel as IDisposable)?.Dispose();
        }
    }
}
