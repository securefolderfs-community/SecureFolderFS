using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecureFolderFS.Backend.Models;

namespace SecureFolderFS.Backend.ViewModels.Pages.DashboardPages
{
    public sealed class VaultDashboardPropertiesPageViewModel : BaseDashboardPageViewModel
    {
        public VaultDashboardPropertiesPageViewModel(VaultModel vaultModel)
            : base(vaultModel)
        {
        }
    }
}
