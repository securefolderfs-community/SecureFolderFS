using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.AppModels
{
    internal sealed class CachingNavigationModel<TViewModel> : INavigationModel
        where TViewModel : class, INotifyPropertyChanged, IEquatable<TViewModel>
    {
        public CachingNavigationModel(IMessenger messenger)
        {
            messenger.Register<NavigationRequestedMessage<TViewModel>>(this, (s, e) => RegisterNavigation(e.ViewModel));
        }

        private void RegisterNavigation(TViewModel viewModel)
        {

        }

        public void ResetNavigation()
        {
            throw new NotImplementedException();
        }

        public void ResetNavigation<TViewModel1>(TViewModel1 viewModel) where TViewModel1 : class, INotifyPropertyChanged, IEquatable<TViewModel1>
        {
            throw new NotImplementedException();
        }
    }
}
