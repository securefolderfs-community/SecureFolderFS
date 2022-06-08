using SecureFolderFS.Sdk.Models.Transitions;
using SecureFolderFS.Sdk.ViewModels.Pages.DashboardPages;

namespace SecureFolderFS.Sdk.Messages
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
