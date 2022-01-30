using CommunityToolkit.Mvvm.Messaging.Messages;
using SecureFolderFS.Backend.ViewModels.Pages.DashboardPages;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class DashboardNavigationFinishedMessage : ValueChangedMessage<BaseDashboardPageViewModel>
    {
        public string? From { get; init; }

        public DashboardNavigationFinishedMessage(BaseDashboardPageViewModel value)
            : base(value)
        {
        }
    }
}
