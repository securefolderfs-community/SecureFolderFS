using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Shared;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    [Inject<ISettingsService>, Inject<ISystemService>]
    public sealed partial class SystemMonitorModel : ISystemMonitorModel
    {
        /// <inheritdoc/>
        public IVaultCollectionModel VaultCollectionModel { get; }

        public SystemMonitorModel(IVaultCollectionModel vaultCollectionModel)
        {
            ServiceProvider = DI.Default;
            VaultCollectionModel = vaultCollectionModel;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            SettingsService.UserSettings.PropertyChanged += UserSettings_PropertyChanged;
            AttachEvents();

            return Task.CompletedTask;
        }

        private void AttachEvents()
        {
            if (SettingsService.UserSettings.LockOnSystemLock)
                SystemService.DesktopLocked += SystemService_DesktopLocked;
            else
                SystemService.DesktopLocked -= SystemService_DesktopLocked;
        }

        private void SystemService_DesktopLocked(object? sender, EventArgs e)
        {
            if (!SettingsService.UserSettings.LockOnSystemLock)
                return;

            foreach (var item in VaultCollectionModel)
            {
                WeakReferenceMessenger.Default.Send(new VaultLockRequestedMessage(item));
            }
        }

        private void UserSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IUserSettings.LockOnSystemLock))
                return;

            AttachEvents();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            SystemService.DesktopLocked -= SystemService_DesktopLocked;
            SettingsService.UserSettings.PropertyChanged -= UserSettings_PropertyChanged;
        }
    }
}
