﻿using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Credentials;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    public sealed partial class CredentialsOverlayViewModel : OverlayViewModel, IAsyncInitialize, IDisposable
    {
        private readonly LoginViewModel _loginViewModel;
        private readonly CredentialsSelectionViewModel _selectionViewModel;

        [ObservableProperty] private INotifyPropertyChanged? _SelectedViewModel;

        public CredentialsOverlayViewModel(IVaultModel vaultModel, CredentialsSelectionViewModel selectionViewModel)
        {
            _loginViewModel = new(vaultModel, false);
            _selectionViewModel = selectionViewModel;

            SelectedViewModel = _loginViewModel;
            Title = "Authenticate".ToLocalized();
            PrimaryButtonText = "Continue".ToLocalized();

            _loginViewModel.VaultUnlocked += LoginViewModel_VaultUnlocked;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await _selectionViewModel.InitAsync(cancellationToken);
            await _loginViewModel.InitAsync(cancellationToken);
        }

        private void LoginViewModel_VaultUnlocked(object? sender, VaultUnlockedEventArgs e)
        {
            Title = "Select authentication option";
            PrimaryButtonText = null;
            SelectedViewModel = _selectionViewModel;
            _ = e;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _selectionViewModel.Dispose();
            _loginViewModel.Dispose();
        }
    }
}