using CommunityToolkit.Mvvm.Messaging.Messages;
using SecureFolderFS.Backend.Models.Transitions;
using SecureFolderFS.Backend.ViewModels.Pages.DashboardPages;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class DashboardNavigationFinishedMessage : ValueChangedMessage<BaseDashboardPageViewModel>
    {
        public TransitionModel? Transition { get; init; }

        public DashboardNavigationFinishedMessage(BaseDashboardPageViewModel value)
            : base(value)
        {
        }
    }
}
