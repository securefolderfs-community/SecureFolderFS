﻿using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Sdk.ViewModels.Dialogs;

namespace SecureFolderFS.Sdk.ViewModels.Sidebar
{
    public sealed class SidebarFooterViewModel : ObservableObject
    {
        private IDialogService DialogService { get; } = Ioc.Default.GetRequiredService<IDialogService>();

        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        public IAsyncRelayCommand AddNewVaultCommand { get; }

        public IAsyncRelayCommand OpenSettingsCommand { get; }

        public SidebarFooterViewModel()
        {
            OpenSettingsCommand = new AsyncRelayCommand(OpenSettingsAsync);
            AddNewVaultCommand = new AsyncRelayCommand(AddNewVaultAsync);
        }

        private async Task AddNewVaultAsync()
        {
            await DialogService.ShowDialogAsync(new VaultWizardDialogViewModel());
            await SettingsService.SaveSettingsAsync();
        }

        private async Task OpenSettingsAsync()
        {
            await DialogService.ShowDialogAsync(new SettingsDialogViewModel());
            await SettingsService.SaveSettingsAsync();
        }
    }
}