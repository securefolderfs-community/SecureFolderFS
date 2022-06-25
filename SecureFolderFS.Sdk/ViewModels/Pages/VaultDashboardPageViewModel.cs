using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Models.Transitions;
using SecureFolderFS.Sdk.ViewModels.Dashboard.Navigation;
using SecureFolderFS.Sdk.ViewModels.Pages.DashboardPages;
using SecureFolderFS.Core.VaultLoader.Routine;

namespace SecureFolderFS.Sdk.ViewModels.Pages
{
    public sealed class VaultDashboardPageViewModel : BasePageViewModel, IRecipient<DashboardNavigationFinishedMessage>
    {
        public NavigationBreadcrumbViewModel NavigationBreadcrumbViewModel { get; }

        public DashboardNavigationModel DashboardNavigationModel { get; }

        public BaseDashboardPageViewModel? CurrentPage { get; private set; }

        public VaultDashboardPageViewModel(IMessenger messenger, VaultViewModelDeprecated vaultViewModel)
            : base(messenger, vaultViewModel)
        {
            NavigationBreadcrumbViewModel = new();
            DashboardNavigationModel = new(Messenger);

            Messenger.Register<DashboardNavigationFinishedMessage>(this);
            Messenger.Register<DashboardNavigationFinishedMessage>(NavigationBreadcrumbViewModel);
            Messenger.Register<DashboardNavigationRequestedMessage>(DashboardNavigationModel);
            Messenger.Register<VaultLockedMessage>(DashboardNavigationModel);
        }

        public void InitializeWithRoutine(IFinalizedVaultLoadRoutine finalizedVaultLoadRoutine)
        {
            if (CurrentPage is VaultMainDashboardPageViewModel viewModel)
            {
                finalizedVaultLoadRoutine = finalizedVaultLoadRoutine.ContinueWithOptionalRoutine()
                    .EstablishOptionalRoutine()
                    .AddFileSystemStatsTracker(viewModel.GraphsWidgetViewModel.VaultIoSpeedReporterModel)
                    .Finalize();

                VaultViewModel.VaultInstance = finalizedVaultLoadRoutine.Deploy();
                AsyncExtensions.RunAndForget(() =>
                {
                    VaultViewModel.VaultInstance.SecureFolderFSInstance.StartFileSystem();
                });
            }
        }

        public void StartNavigation()
        {
            Messenger.Send(new DashboardNavigationRequestedMessage(CurrentPage?.VaultDashboardPageType ?? VaultDashboardPageType.MainDashboardPage, VaultViewModel, CurrentPage)
            {
                Transition = CurrentPage is null ? new ContinuumTransitionModel() : new SuppressTransitionModel()
            });
        }

        public void Receive(DashboardNavigationFinishedMessage message)
        {
            CurrentPage = message.Value;
        }

        public override void Cleanup()
        {
            CurrentPage?.Cleanup();
            base.Cleanup();
        }

        public override void Dispose()
        {
            CurrentPage?.Dispose();
            base.Dispose();
        }
    }
}
