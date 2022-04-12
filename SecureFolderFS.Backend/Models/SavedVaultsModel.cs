using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Services;
using SecureFolderFS.Backend.Utils;
using SecureFolderFS.Backend.ViewModels;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Backend.Services.Settings;

#nullable enable

namespace SecureFolderFS.Backend.Models
{
    public sealed class SavedVaultsModel : IRecipient<AddVaultRequestedMessage>, IRecipient<RemoveVaultRequestedMessage>, IRecipient<VaultSerializationRequestedMessage>
    {
        private IConfidentialStorageService ConfidentialStorageService { get; } = Ioc.Default.GetRequiredService<IConfidentialStorageService>();

        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        internal IInitializableSource<IDictionary<VaultIdModel, VaultViewModel>>? InitializableSource { get; init; }

        public SavedVaultsModel()
        {
            WeakReferenceMessenger.Default.Register<AddVaultRequestedMessage>(this);
            WeakReferenceMessenger.Default.Register<RemoveVaultRequestedMessage>(this);
            WeakReferenceMessenger.Default.Register<VaultSerializationRequestedMessage>(this);
        }

        public void Initialize()
        {
            if (InitializableSource != null && (SettingsService.IsAvailable && ConfidentialStorageService.IsAvailable))
            {
                var savedVaults = SettingsService.SavedVaults;
                var savedVaultModels = ConfidentialStorageService.SavedVaultModels;

                foreach (var item in savedVaults.Keys)
                {
                    savedVaults[item].VaultModel = savedVaultModels.TryGetValue(item, out var model) ? model : new(item);
                }

                InitializableSource.Initialize(savedVaults);
            }
        }

        public void Receive(AddVaultRequestedMessage message)
        {
            if (SettingsService.IsAvailable && ConfidentialStorageService.IsAvailable)
            {
                var savedVaults = SettingsService.SavedVaults!.ToDictionary();
                var savedVaultModels = ConfidentialStorageService.SavedVaultModels!.ToDictionary();

                savedVaults.Add(message.Value.VaultIdModel, message.Value);
                savedVaultModels.Add(message.Value.VaultIdModel, message.Value.VaultModel);

                SettingsService.SavedVaults = savedVaults!;
                ConfidentialStorageService.SavedVaultModels = savedVaultModels!;
            }
        }

        public void Receive(RemoveVaultRequestedMessage message)
        {
            if (SettingsService.IsAvailable && ConfidentialStorageService.IsAvailable)
            {
                var savedVaults = SettingsService.SavedVaults!.ToDictionary();
                var savedVaultModels = ConfidentialStorageService.SavedVaultModels!.ToDictionary();

                savedVaults.Remove(message.Value);
                savedVaultModels.Remove(message.Value);

                SettingsService.SavedVaults = savedVaults!;
                ConfidentialStorageService.SavedVaultModels = savedVaultModels!;
            }
        }

        public void Receive(VaultSerializationRequestedMessage message)
        {
            if (SettingsService.IsAvailable && ConfidentialStorageService.IsAvailable)
            {
                var savedVaults = SettingsService.SavedVaults!.ToDictionary();
                var savedVaultModels = ConfidentialStorageService.SavedVaultModels!.ToDictionary();

                if (savedVaults.ContainsKey(message.Value.VaultIdModel))
                {
                    savedVaults[message.Value.VaultIdModel] = message.Value;
                }
                if (savedVaultModels.ContainsKey(message.Value.VaultIdModel))
                {
                    savedVaultModels[message.Value.VaultIdModel] = message.Value.VaultModel;

                }

                SettingsService.SavedVaults = savedVaults!;
                ConfidentialStorageService.SavedVaultModels = savedVaultModels!;
            }
        }
    }
}
