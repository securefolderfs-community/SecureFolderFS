using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models.Transitions;
using SecureFolderFS.WinUI.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls
{
    public partial class NavigationControl : UserControl, IRecipient<NavigationRequestedMessage>
    {
        public Dictionary<Type, Type>? ViewModelAssociation { get; set; }

        public NavigationControl()
        {
            InitializeComponent();
        }

        public void Navigate<TViewModel>(TViewModel viewModel)
            where TViewModel : class, INotifyPropertyChanged
        {
            Navigate(viewModel, new SuppressTransitionModel());
        }

        public void Navigate<TViewModel>(TViewModel viewModel, TransitionModel transition)
            where TViewModel : class, INotifyPropertyChanged
        {
            ArgumentNullException.ThrowIfNull(ViewModelAssociation);

            var viewModelType = viewModel.GetType();
            if (ViewModelAssociation.TryGetValue(viewModelType, out var pageType))
            {
                var transitionInfo = ConversionHelpers.ToNavigationTransitionInfo(transition);
                ContentFrame.Navigate(pageType, viewModel, transitionInfo);
            }
        }

        void IRecipient<NavigationRequestedMessage>.Receive(NavigationRequestedMessage message)
        {
            Navigate(message.ViewModel, message.Transition ?? new SuppressTransitionModel());
        }
    }
}
