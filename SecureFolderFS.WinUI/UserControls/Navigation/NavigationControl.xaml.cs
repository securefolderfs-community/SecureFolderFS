using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.Messages.Navigation;
using System;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls.Navigation
{
    /// <summary>
    /// The base class to manage navigation of pages using messages.
    /// </summary>
    public abstract partial class NavigationControl : UserControl, IRecipient<NavigationMessage>, IRecipient<BackNavigationMessage>, IDisposable
    {
        protected NavigationControl()
        {
            InitializeComponent();
        }

        /// <inheritoc/>
        public virtual void Receive(NavigationMessage message)
        {
            Navigate(message.ViewModel, null);
        }

        /// <inheritoc/>
        public virtual void Receive(BackNavigationMessage message)
        {
            _ = message;
        }

        public abstract void Navigate<TViewModel>(TViewModel viewModel, NavigationTransitionInfo? transitionInfo) where TViewModel : INotifyPropertyChanged;

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            (ContentFrame.Content as IDisposable)?.Dispose();
        }
    }
}
