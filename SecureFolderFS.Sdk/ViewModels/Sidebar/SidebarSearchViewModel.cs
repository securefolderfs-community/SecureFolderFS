using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models.Search;

namespace SecureFolderFS.Sdk.ViewModels.Sidebar
{
    public sealed class SidebarSearchViewModel : ObservableObject
    {
        private ISearchModel SearchModel { get; }

        public ObservableCollection<string> SearchItems { get; }

        public SidebarSearchViewModel(IEnumerable<SidebarItemViewModel> sidebarItems)
        {
            SearchItems = new();
            SearchModel = new SidebarSearchModel<SidebarItemViewModel>(sidebarItems);
        }

        public async Task SubmitQuery(string query)
        {
            SearchItems.Clear();
            await foreach (var item in SearchModel.SearchAsync(query))
            {
                SearchItems.Add((item as SidebarItemViewModel)!.VaultName);
            }
        }
    }
}
