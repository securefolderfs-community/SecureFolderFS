using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Extensions;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.ViewModels.Pages;

#nullable enable

namespace SecureFolderFS.Backend.Models
{
    public sealed class NavigationModel : IRecipient<RemoveVaultRequestedMessage>, IRecipient<AddVaultRequestedMessage>
    {
        public Dictionary<VaultModel, BasePageViewModel?> NavigationDestinations { get; }

        public NavigationModel()
        {
            NavigationDestinations = new();

            WeakReferenceMessenger.Default.Register<RemoveVaultRequestedMessage>(this);
            WeakReferenceMessenger.Default.Register<AddVaultRequestedMessage>(this);
        }

        public void NavigateToPage(VaultModel? vaultModel)
        {
            if (vaultModel == null) return;

            NavigationDestinations.SetAndGet(vaultModel, out var basePageViewModel, () => new VaultLoginPageViewModel(vaultModel, this));

            WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(basePageViewModel!));
        }

        public void NavigateToPage(VaultModel? vaultModel, BasePageViewModel basePageViewModel)
        {
            if (vaultModel == null) return;

            if (!NavigationDestinations.SetAndGet(vaultModel, out _, () => basePageViewModel))
            {
                // Wasn't updated, do it manually..
                NavigationDestinations[vaultModel] = basePageViewModel;
            }

            WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(basePageViewModel!));
        }

        public void Receive(RemoveVaultRequestedMessage message)
        {
            NavigationDestinations.Remove(message.Value, out var viewModel);
            viewModel?.Dispose();

            var itemToNavigateTo = NavigationDestinations.Keys.FirstOrDefault();
            NavigateToPage(itemToNavigateTo);
        }

        public void Receive(AddVaultRequestedMessage message)
        {
            NavigationDestinations.AddOrSet(message.Value);
        }
    }
}
