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
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Dialogs;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Inject<IThreadingService>, Inject<IVaultService>, Inject<IDialogService>, Inject<ISettingsService>]
    public sealed partial class VaultLoginPageViewModel : BaseVaultPageViewModel
    {
        [ObservableProperty] private string? _VaultName;
        [ObservableProperty] private LoginViewModel _LoginViewModel;

        public VaultLoginPageViewModel(VaultViewModel vaultViewModel, INavigationService navigationService)
            : base(vaultViewModel, navigationService)
        {
            ServiceProvider = Ioc.Default;
            _LoginViewModel = new(vaultViewModel.VaultModel, true);
            VaultName = vaultViewModel.VaultModel.VaultName;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Move to event handler where successful unlock is going to be handled:
            if (!SettingsService.AppSettings.WasVaultFolderExplanationShown)
            {
                var explanationDialog = new ExplanationDialogViewModel();
                await explanationDialog.InitAsync(cancellationToken);
                await DialogService.ShowDialogAsync(explanationDialog);

                SettingsService.AppSettings.WasVaultFolderExplanationShown = true;
                await SettingsService.AppSettings.TrySaveAsync(cancellationToken);
            }
        }





        

        /// <inheritdoc/>
        public override void Dispose()
        {
            LoginViewModel.Dispose();
        }
    }
}
