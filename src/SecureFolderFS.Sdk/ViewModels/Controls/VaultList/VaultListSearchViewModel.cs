using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.VaultList
{
    public sealed partial class VaultListSearchViewModel(IEnumerable<VaultListItemViewModel> sidebarItems) : ObservableObject
    {
        private ISearchModel SearchModel { get; } = new SidebarSearchModel(sidebarItems);

        public ObservableCollection<string> SearchItems { get; } = new();

        [RelayCommand]
        public async Task SubmitQueryAsync(string query, CancellationToken cancellationToken = default)
        {
            SearchItems.Clear();
            await foreach (var item in SearchModel.SearchAsync(query, cancellationToken))
            {
                if (item is VaultListItemViewModel sidebarItem)
                    SearchItems.Add(sidebarItem.VaultViewModel.VaultModel.VaultName);
            }
        }
    }
}
