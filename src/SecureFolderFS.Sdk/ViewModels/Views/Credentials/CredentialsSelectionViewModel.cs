﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Sdk.ViewModels.Views.Credentials
{
    [Inject<IVaultService>]
    [Bindable(true)]
    public sealed partial class CredentialsSelectionViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        private readonly IFolder _vaultFolder;
        private readonly AuthenticationType _allowedStage;

        [ObservableProperty] private AuthenticationViewModel? _CurrentViewModel;
        [ObservableProperty] private ObservableCollection<AuthenticationViewModel> _AuthenticationOptions;

        public CredentialsSelectionViewModel(IFolder vaultFolder, AuthenticationViewModel currentViewModel)
            : this(vaultFolder, currentViewModel.Availability)
        {
            CurrentViewModel = currentViewModel;
        }

        public CredentialsSelectionViewModel(IFolder vaultFolder, AuthenticationType allowedStage)
        {
            ServiceProvider = Ioc.Default;
            _vaultFolder = vaultFolder;
            _allowedStage = allowedStage;
            AuthenticationOptions = new();
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Get authentication options
            var vaultOptions = await VaultService.GetVaultOptionsAsync(_vaultFolder, cancellationToken);
            await foreach (var item in VaultService.GetCreationAsync(_vaultFolder, vaultOptions.VaultId!, cancellationToken))
            {
                // Don't add authentication methods to list which are already in use
                if (vaultOptions.AuthenticationMethod.Contains(item.Id))
                    continue;

                // Don't add authentication methods which are not allowed in the _allowedStage
                if (!item.Availability.HasFlag(_allowedStage))
                    continue;

                AuthenticationOptions.Add(item);
            }
        }

        [RelayCommand]
        private async Task RemoveCredentialsAsync(CancellationToken cancellationToken)
        {

        }

        /// <inheritdoc/>
        public void Dispose()
        {
            CurrentViewModel?.Dispose();
            AuthenticationOptions.DisposeElements();
        }
    }
}