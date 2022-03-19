using SecureFolderFS.Backend.Models.Transitions;
using SecureFolderFS.Backend.ViewModels.Pages.DashboardPages;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class DashboardNavigationFinishedMessage : ValueMessage<BaseDashboardPageViewModel>
    {
        public TransitionModel? Transition { get; init; }

        public DashboardNavigationFinishedMessage(BaseDashboardPageViewModel value)
            : base(value)
        {
        }
    }
}
