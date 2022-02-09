using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Backend.Extensions;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.Models.Transitions;
using SecureFolderFS.Backend.ViewModels.Pages;
using SecureFolderFS.WinUI.Helpers;
using SecureFolderFS.WinUI.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

#nullable enable

namespace SecureFolderFS.WinUI.UserControls
{
    public sealed partial class NavigationControl : UserControl,
        IRecipient<LockVaultRequestedMessage>,
        IRecipient<NavigationRequestedMessage>,
        IRecipient<RemoveVaultRequestedMessage>,
        IRecipient<AddVaultRequestedMessage>
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
            Navigate(message.VaultModel, message.Value, message.Transition);
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
            Navigate(message.Value, null, new DrillOutTransitionModel());
        }

        private void Navigate(VaultModel vaultModel, BasePageViewModel? basePageViewModel, TransitionModel? transition = null)
        {
            if (basePageViewModel == null)
            {
                NavigationDestinations.SetAndGet(vaultModel, out basePageViewModel, () => new VaultLoginPageViewModel(vaultModel));
                PageViewModel = basePageViewModel!;
                if (!PageViewModel.Messenger.IsRegistered<LockVaultRequestedMessage>(this))
                {
                    PageViewModel.Messenger.Register<LockVaultRequestedMessage>(this);
                }
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

            Navigate(PageViewModel, transition);

            WeakReferenceMessenger.Default.Send(new NavigationFinishedMessage(PageViewModel));
        }

        private async void Navigate(BasePageViewModel basePageViewModel, TransitionModel? transition = null)
        {
            var transitionInfo = ConversionHelpers.ToNavigationTransitionInfo(transition) ?? new EntranceNavigationTransitionInfo();

            if (transition?.IsCustom ?? false)
            {
                if (transition is DrillOutTransitionModel)
                {
                    DrillOutAnimationStoryboard.Begin();
                    await Task.Delay(200);
                    DrillOutAnimationStoryboard.Stop();
                    transitionInfo = new EntranceNavigationTransitionInfo();
                }
            }

            switch (basePageViewModel)
            {
                case VaultLoginPageViewModel:
                    ContentFrame.Navigate(typeof(VaultLoginPage), basePageViewModel, transitionInfo);
                    break;

                case VaultDashboardPageViewModel:
                    ContentFrame.Navigate(typeof(VaultDashboardPage), basePageViewModel, transitionInfo);
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
