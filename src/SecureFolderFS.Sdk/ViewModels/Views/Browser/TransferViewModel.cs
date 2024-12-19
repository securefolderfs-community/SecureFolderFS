using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Browser
{
    public sealed partial class TransferViewModel : ObservableObject, IViewable
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private bool _IsProgressing;
        [ObservableProperty] private ObservableCollection<BrowserItemViewModel> _TranferredItems;

        private async Task RegisterTransferAsync(IEnumerable<BrowserItemViewModel> items, CancellationToken cancellationToken)
        {
            var transferType = TransferType.Copy;
            switch (transferType)
            {
                case TransferType.Copy:
                {
                    break;
                }

                case TransferType.Move:
                {
                    break;
                }
                
                default: return;
            }
        }
    }
}
