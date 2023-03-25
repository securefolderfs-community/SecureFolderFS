using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard;
using System;
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
        protected override Task<DashboardPageType> BeginNavigationAsync(INavigationTarget target, NavigationType navigationType)
        {
            var identifier = target switch
            {
                VaultOverviewPageViewModel => DashboardPageType.OverviewPage,
                VaultPropertiesPageViewModel => DashboardPageType.PropertiesPage,
                _ => throw new ArgumentOutOfRangeException(nameof(target))
            };

            _messenger.Send(new NavigationRequestedMessage(target));
            return Task.FromResult(identifier);
        }
    }
}
