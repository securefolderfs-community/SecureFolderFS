using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models.Transitions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls.Navigation
{
    public abstract partial class NavigationControl : UserControl, IRecipient<NavigationRequestedMessage>
    {
        protected NavigationControl()
        {
            InitializeComponent();
        }

        public virtual void Receive(NavigationRequestedMessage message)
        {
            Navigate(message.ViewModel, message.Transition ?? new SuppressTransitionModel());
        }

        public abstract void Navigate<TViewModel>(TViewModel viewModel, TransitionModel? transition) where TViewModel : class, INotifyPropertyChanged;
    }
}
