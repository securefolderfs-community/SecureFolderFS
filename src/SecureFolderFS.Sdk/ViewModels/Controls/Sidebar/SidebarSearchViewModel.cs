using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Sidebar
{
    public sealed class SidebarSearchViewModel : ObservableObject
    {
        private ISearchModel SearchModel { get; }

        public ObservableCollection<string> SearchItems { get; }

        public SidebarSearchViewModel(IEnumerable<SidebarItemViewModel> sidebarItems)
        {
            SearchItems = new();
            SearchModel = new SidebarSearchModel(sidebarItems);
        }

        public async Task SubmitQuery(string query, CancellationToken cancellationToken = default)
        {
            SearchItems.Clear();
            await foreach (var item in SearchModel.SearchAsync(query, cancellationToken))
            {
                if (item is SidebarItemViewModel sidebarItem)
                    SearchItems.Add(sidebarItem.VaultViewModel.VaultModel.VaultName);
            }
        }
    }
}
