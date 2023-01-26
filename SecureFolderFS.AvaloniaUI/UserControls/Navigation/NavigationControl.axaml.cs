using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using SecureFolderFS.Sdk.Messages.Navigation;

namespace SecureFolderFS.AvaloniaUI.UserControls.Navigation
{
    /// <summary>
    /// The base class to manage navigation of pages using messages.
    /// </summary>
    internal partial class NavigationControl : UserControl, IDisposable, IRecipient<NavigationRequestedMessage>, IRecipient<BackNavigationRequestedMessage>
    {
        public NavigationControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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

        public virtual void Navigate<TViewModel>(TViewModel viewModel, NavigationTransitionInfo? transitionInfo)
            where TViewModel : class, INotifyPropertyChanged
        {
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            (ContentFrame.Content as IDisposable)?.Dispose();
        }

        private void ContentFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            if (ContentFrame.Content is Page page)
                page.OnNavigatedTo(e);
        }

        private void ContentFrame_OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (ContentFrame.Content is Page page)
                page.OnNavigatingFrom(e);
        }
    }
}