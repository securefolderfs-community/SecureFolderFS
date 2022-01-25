using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;
using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Backend.ViewModels.Pages.DashboardPages;

namespace SecureFolderFS.Backend.Messages
{
    public sealed class DashboardNavigationRequestedMessage : ValueChangedMessage<BaseDashboardPageViewModel>
    {
        public NavigationType NavigationType { get; init; }

        public DashboardNavigationRequestedMessage(BaseDashboardPageViewModel value)
            : base(value)
        {
        }
    }
}
