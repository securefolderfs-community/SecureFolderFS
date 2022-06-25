using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Utils;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Sdk.Services.Settings;

namespace SecureFolderFS.Sdk.Models
{
    [Obsolete("This class has been deprecated. Use IVaultCollectionModel instead.")]
    public sealed class SavedVaultsModel : IRecipient<AddVaultRequestedMessageDeprecated>, IRecipient<RemoveVaultRequestedMessageDeprecated>, IRecipient<VaultSerializationRequestedMessage>
    {
        private ISecretSettingsService SecretSettingsService { get; } = Ioc.Default.GetRequiredService<ISecretSettingsService>();

        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        internal IInitializableSource<IDictionary<VaultIdModel, VaultViewModelDeprecated>>? InitializableSource { get; init; }

        public SavedVaultsModel()
        {
            WeakReferenceMessenger.Default.Register<AddVaultRequestedMessageDeprecated>(this);
            WeakReferenceMessenger.Default.Register<RemoveVaultRequestedMessageDeprecated>(this);
            WeakReferenceMessenger.Default.Register<VaultSerializationRequestedMessage>(this);
        }

        public void Initialize()
        {
            if (InitializableSource is not null && (SettingsService.IsAvailable && SecretSettingsService.IsAvailable))
            {
                var savedVaults = SettingsService.SavedVaults;
                var savedVaultModels = SecretSettingsService.SavedVaultModels;

                foreach (var item in savedVaults.Keys)
                {
                    savedVaults[item].VaultModelDeprecated = savedVaultModels.TryGetValue(item, out var model) ? model : new(item);
                }

                InitializableSource.Initialize(savedVaults);
            }
        }

        public void Receive(AddVaultRequestedMessageDeprecated message)
        {
            if (SettingsService.IsAvailable && SecretSettingsService.IsAvailable)
            {
                var savedVaults = SettingsService.SavedVaults!.ToDictionary();
                var savedVaultModels = SecretSettingsService.SavedVaultModels!.ToDictionary();

                savedVaults.Add(message.Value.VaultIdModel, message.Value);
                savedVaultModels.Add(message.Value.VaultIdModel, message.Value.VaultModelDeprecated);

                SettingsService.SavedVaults = savedVaults!;
                SecretSettingsService.SavedVaultModels = savedVaultModels!;
            }
        }

        public void Receive(RemoveVaultRequestedMessageDeprecated message)
        {
            if (SettingsService.IsAvailable && SecretSettingsService.IsAvailable)
            {
                var savedVaults = SettingsService.SavedVaults!.ToDictionary();
                var savedVaultModels = SecretSettingsService.SavedVaultModels!.ToDictionary();

                savedVaults.Remove(message.Value);
                savedVaultModels.Remove(message.Value);

                SettingsService.SavedVaults = savedVaults!;
                SecretSettingsService.SavedVaultModels = savedVaultModels!;
            }
        }

        public void Receive(VaultSerializationRequestedMessage message)
        {
            if (SettingsService.IsAvailable && SecretSettingsService.IsAvailable)
            {
                var savedVaults = SettingsService.SavedVaults!.ToDictionary();
                var savedVaultModels = SecretSettingsService.SavedVaultModels!.ToDictionary();

                if (savedVaults.ContainsKey(message.Value.VaultIdModel))
                {
                    savedVaults[message.Value.VaultIdModel] = message.Value;
                }
                if (savedVaultModels.ContainsKey(message.Value.VaultIdModel))
                {
                    savedVaultModels[message.Value.VaultIdModel] = message.Value.VaultModelDeprecated;

                }

                SettingsService.SavedVaults = savedVaults!;
                SecretSettingsService.SavedVaultModels = savedVaultModels!;
            }
        }
    }
}
