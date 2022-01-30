using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Backend.Extensions;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.ViewModels.Pages;
using SecureFolderFS.WinUI.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

#nullable enable

namespace SecureFolderFS.WinUI.UserControls
{
    public sealed partial class NavigationControl : UserControl, IRecipient<LockVaultRequestedMessage>, IRecipient<NavigationRequestedMessage>, IRecipient<RemoveVaultRequestedMessage>, IRecipient<AddVaultRequestedMessage>
    {
        private Dictionary<VaultModel, BasePageViewModel?> NavigationDestinations { get; }

        public NavigationControl()
        {
            this.InitializeComponent();

            this.NavigationDestinations = new();

            WeakReferenceMessenger.Default.Register<NavigationRequestedMessage>(this);
            WeakReferenceMessenger.Default.Register<RemoveVaultRequestedMessage>(this);
            WeakReferenceMessenger.Default.Register<AddVaultRequestedMessage>(this);
            WeakReferenceMessenger.Default.Register<LockVaultRequestedMessage>(this);
        }

        public void Receive(NavigationRequestedMessage message)
        {
            Navigate(message.VaultModel, message.Value);
        }

        public void Receive(RemoveVaultRequestedMessage message)
        {
            NavigationDestinations.Remove(message.Value, out var viewModel);
            viewModel?.Dispose();
        }

        public void Receive(AddVaultRequestedMessage message)
        {
            NavigationDestinations.AddOrSet(message.Value);
        }

        public void Receive(LockVaultRequestedMessage message)
        {
            NavigationDestinations[message.Value]?.Dispose();
            NavigationDestinations[message.Value] = null;
            Navigate(message.Value, null);
        }

        private void Navigate(VaultModel vaultModel, BasePageViewModel? basePageViewModel)
        {
            if (basePageViewModel == null)
            {
                NavigationDestinations.SetAndGet(vaultModel, out basePageViewModel, () => new VaultLoginPageViewModel(vaultModel));
                PageViewModel = basePageViewModel!;
            }
            else
            {
                if (!NavigationDestinations.SetAndGet(vaultModel, out _, () => basePageViewModel))
                {
                    // Wasn't updated, do it manually..
                    NavigationDestinations[vaultModel] = basePageViewModel;
                    PageViewModel = basePageViewModel;
                }
            }

            Navigate(PageViewModel!);

            WeakReferenceMessenger.Default.Send(new NavigationFinishedMessage(PageViewModel!));
        }

        private void Navigate(BasePageViewModel basePageViewModel)
        {
            switch (basePageViewModel)
            {
                case VaultLoginPageViewModel:
                    ContentFrame.Navigate(typeof(VaultLoginPage), new PageNavigationParameterModel() { ViewModel = basePageViewModel }, new EntranceNavigationTransitionInfo());
                    break;

                case VaultDashboardPageViewModel:
                    ContentFrame.Navigate(typeof(VaultDashboardPage), new PageNavigationParameterModel() { ViewModel = basePageViewModel }, new DrillInNavigationTransitionInfo());
                    break;
            }
        }

        public BasePageViewModel PageViewModel
        {
            get => (BasePageViewModel)GetValue(PageViewModelProperty);
            set => SetValue(PageViewModelProperty, value);
        }
        public static readonly DependencyProperty PageViewModelProperty =
            DependencyProperty.Register("PageViewModel", typeof(BasePageViewModel), typeof(NavigationControl), new PropertyMetadata(null));
    }
}
