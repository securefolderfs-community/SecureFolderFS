using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Sdk.ViewModels.Views.Browser
{
    public sealed partial class TransferViewModel : ObservableObject
    {
        [ObservableProperty] private bool _IsTransferring;
        [ObservableProperty] private TransferType _TransferType;
        [ObservableProperty] private ObservableCollection<BrowserItemViewModel> _TranferredItems;

        [RelayCommand]
        private async Task TransferAsync(CancellationToken cancellationToken)
        {
            switch (TransferType)
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
