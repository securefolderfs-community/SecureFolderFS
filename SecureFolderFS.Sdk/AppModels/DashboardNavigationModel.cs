using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IStateNavigationModel"/>
    public sealed class DashboardNavigationModel : CachingNavigationModel
    {
        private readonly IMessenger _messenger;

        public DashboardNavigationModel(IMessenger messenger)
        {
            _messenger = messenger;
        }

        /// <inheritdoc/>
        protected override Task BeginNavigationAsync(INavigationTarget target, NavigationType navigationType)
        {
            _messenger.Send(new NavigationMessage(target));
            return Task.CompletedTask;
        }
    }
}
