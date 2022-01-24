using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.ViewModels.Dashboard.Navigation;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Pages
{
    public class VaultDashboardPageViewModel : BasePageViewModel
    {
        public DashboardNavigationViewModel DashboardNavigationViewModel { get; }

        public VaultDashboardPageViewModel(VaultModel vaultModel)
            : base(vaultModel)
        {
            DashboardNavigationViewModel = new();

            Initialize();
        }

        private void Initialize()
        {
            DashboardNavigationViewModel.AppendNavigation(new()
            {
                SectionName = VaultModel.VaultName,
            });
        }

        public override void Dispose()
        {
        }
    }
}
