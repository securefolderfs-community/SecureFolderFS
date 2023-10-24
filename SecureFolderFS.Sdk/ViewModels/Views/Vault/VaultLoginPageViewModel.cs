using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Vault;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Login;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Inject<IThreadingService>, Inject<IVaultService>]
    public sealed partial class VaultLoginPageViewModel : BaseVaultPageViewModel
    {
        [ObservableProperty] private string? _VaultName;

        public VaultLoginPageViewModel(VaultViewModel vaultViewModel, INavigationService navigationService)
            : base(vaultViewModel, navigationService)
        {
            ServiceProvider = Ioc.Default;
            VaultName = vaultViewModel.VaultModel.VaultName;

        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Initialize vault watcher
            await _vaultWatcherModel.InitAsync(cancellationToken);

            

            // Set up the first authentication method
            if (!await TryNextAuthAsync())
                LoginTypeViewModel = new ErrorViewModel("No authentication methods available");
        }





        

        /// <inheritdoc/>
        public override void Dispose()
        {
            _credentialsBuilder.Dispose();
            _vaultWatcherModel.Dispose();
            _vaultWatcherModel.StateChanged -= VaultWatcherModel_StateChanged;

            if (LoginTypeViewModel is INotifyStateChanged notifyStateChanged)
                notifyStateChanged.StateChanged -= LoginViewModel_StateChanged;
        }
    }
}
