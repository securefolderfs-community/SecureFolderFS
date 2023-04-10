using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IStateNavigationService"/>
    public sealed class DashboardNavigationService : CachingNavigationService
    {
        private readonly IMessenger _messenger;

        public DashboardNavigationService(IMessenger messenger)
        {
            _messenger = messenger;
        }

        /// <inheritdoc/>
        protected override Task BeginNavigationAsync(INavigationTarget target, NavigationType navigationType)
        {
            _messenger.Send(new NavigationMessage(target as INotifyPropertyChanged));
            return Task.CompletedTask;
        }
    }
}
