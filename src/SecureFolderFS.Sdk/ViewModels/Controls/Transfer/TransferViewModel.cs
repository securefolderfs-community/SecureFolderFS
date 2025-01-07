using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Views.Browser;
using SecureFolderFS.Shared.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.AppModels;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Transfer
{
    public sealed partial class TransferViewModel : ObservableObject, IViewable
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private bool _IsProgressing;
        [ObservableProperty] private ObservableCollection<BrowserItemViewModel> _AllItems;

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

    public partial class OperationViewModel : ObservableObject, IViewable, IProgress<TotalProgress>
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private bool _IsProgressing;
        [ObservableProperty] private double _CurrentProgress;
        [ObservableProperty] private ObservableCollection<BrowserItemViewModel> _AllItems;

        public OperationViewModel()
        {
            AllItems = new();
        }

        public void Report(TotalProgress value)
        {
            throw new NotImplementedException();
        }
    }
}
