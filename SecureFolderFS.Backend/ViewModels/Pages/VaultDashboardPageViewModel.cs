using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.ViewModels.Dashboard.Navigation;
using SecureFolderFS.Backend.ViewModels.Pages.DashboardPages;
using SecureFolderFS.Core.VaultLoader.Routine;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Pages
{
    public class VaultDashboardPageViewModel : BasePageViewModel, IRecipient<DashboardNavigationFinishedMessage>
    {
        public UnlockedVaultModel UnlockedVaultModel { get; }

        public DashboardNavigationViewModel DashboardNavigationViewModel { get; }

        public DashboardNavigationModel DashboardNavigationModel { get; }

        public BaseDashboardPageViewModel? BaseDashboardPageViewModel { get; private set; }

        public VaultDashboardPageViewModel(VaultModel vaultModel)
            : base(vaultModel)
        {
            UnlockedVaultModel = new(vaultModel);
            DashboardNavigationViewModel = new(vaultModel);
            DashboardNavigationModel = new(UnlockedVaultModel);

            WeakReferenceMessenger.Default.Register<DashboardNavigationFinishedMessage>(this);

            Initialize();
        }

        public void InitializeWithFinalizedVaultLoadRoutine(IFinalizedVaultLoadRoutine finalizedVaultLoadRoutine)
        {
            if (BaseDashboardPageViewModel is VaultMainDashboardPageViewModel vaultMainDashboardPageViewModel)
            {
                finalizedVaultLoadRoutine = finalizedVaultLoadRoutine.ContinueWithOptionalRoutine()
                    .EstablishOptionalRoutine()
                    .AddFileSystemStatsTracker(vaultMainDashboardPageViewModel.VaultIoSpeedReporterModel)
                    .Finish();

                UnlockedVaultModel.VaultInstance = finalizedVaultLoadRoutine.Deploy();
                UnlockedVaultModel.StartFileSystem();
            }
        }

        private void Initialize()
        {
            WeakReferenceMessenger.Default.Send(new DashboardNavigationRequestedMessage(VaultDashboardPageType.MainDashboardPage, UnlockedVaultModel));
        }

        public override void Dispose()
        {
        }

        public void Receive(DashboardNavigationFinishedMessage message)
        {
            if (message.Value.UnlockedVaultModel != UnlockedVaultModel)
            {
                return;
            }

            BaseDashboardPageViewModel = message.Value;
        }
    }
}
