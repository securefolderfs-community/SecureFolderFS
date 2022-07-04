using System;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.Messages.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls.Navigation
{
    public abstract partial class NavigationControl : UserControl, IDisposable, IRecipient<NavigationRequestedMessage>, IRecipient<BackNavigationRequestedMessage>
    {
        protected NavigationControl()
        {
            InitializeComponent();
        }

        /// <inheritoc/>
        public virtual void Receive(NavigationRequestedMessage message)
        {
            Navigate(message.ViewModel, null);
        }

        /// <inheritoc/>
        public virtual void Receive(BackNavigationRequestedMessage message)
        {
            _ = message;
        }

        public abstract void Navigate<TViewModel>(TViewModel viewModel, NavigationTransitionInfo? transitionInfo) where TViewModel : class, INotifyPropertyChanged;

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            (ContentFrame.Content as IDisposable)?.Dispose();
        }
    }
}
