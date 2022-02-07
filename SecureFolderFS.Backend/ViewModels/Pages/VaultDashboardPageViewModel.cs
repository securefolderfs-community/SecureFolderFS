using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.Models.Transitions;
using SecureFolderFS.Backend.ViewModels.Dashboard.Navigation;
using SecureFolderFS.Backend.ViewModels.Pages.DashboardPages;
using SecureFolderFS.Core.VaultLoader.Routine;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Pages
{
    public class VaultDashboardPageViewModel : BasePageViewModel, IRecipient<DashboardNavigationFinishedMessage>
    {
        public UnlockedVaultModel UnlockedVaultModel { get; }

        public NavigationBreadcrumbViewModel NavigationBreadcrumbViewModel { get; }

        public DashboardNavigationModel DashboardNavigationModel { get; }

        public BaseDashboardPageViewModel? CurrentPage { get; private set; }

        public VaultDashboardPageViewModel(IMessenger messenger, VaultModel vaultModel)
            : base(messenger, vaultModel)
        {
            this.UnlockedVaultModel = new(vaultModel);
            this.NavigationBreadcrumbViewModel = new();
            this.DashboardNavigationModel = new(Messenger);

            Messenger.Register<DashboardNavigationFinishedMessage>(this);
            Messenger.Register<DashboardNavigationFinishedMessage>(NavigationBreadcrumbViewModel);
            Messenger.Register<DashboardNavigationRequestedMessage>(DashboardNavigationModel);
            Messenger.Register<LockVaultRequestedMessage>(DashboardNavigationModel);
        }

        public void InitializeWithRoutine(IFinalizedVaultLoadRoutine finalizedVaultLoadRoutine)
        {
            if (CurrentPage is VaultMainDashboardPageViewModel viewModel)
            {
                finalizedVaultLoadRoutine = finalizedVaultLoadRoutine.ContinueWithOptionalRoutine()
                    .EstablishOptionalRoutine()
                    .AddFileSystemStatsTracker(viewModel.VaultIoSpeedReporterModel)
                    .Finish();

                UnlockedVaultModel.VaultInstance = finalizedVaultLoadRoutine.Deploy();
                UnlockedVaultModel.StartFileSystem();
            }
        }

        public void StartNavigation()
        {
            Messenger.Send(new DashboardNavigationRequestedMessage(CurrentPage?.VaultDashboardPageType ?? VaultDashboardPageType.MainDashboardPage, UnlockedVaultModel, CurrentPage)
            {
                Transition = CurrentPage == null ? new ContinuumTransitionModel() : new SuppressTransitionModel()
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
    }
}
