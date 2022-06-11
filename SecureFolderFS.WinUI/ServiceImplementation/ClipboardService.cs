using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.WinUI.Models;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    internal sealed class ClipboardService : IClipboardService
    {
        /// <inheritdoc/>
        public Task<bool> IsClipboardAvailableAsync()
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> SetClipboardDataAsync(IClipboardDataModel data)
        {
            return SetClipboardDataInternalAsync(data);
        }

        /// <inheritdoc/>
        public Task<IClipboardDataModel?> RequestClipboardDataAsync()
        {
            return Task.FromResult(GetClipboardDataInternal());
        }

        /// <inheritdoc/>
        public Task<IEnumerable<IClipboardDataModel>?> RequestFullClipboardDataAsync()
        {
            return GetFullClipboardDataInternalAsync();
        }

        private async Task<bool> SetClipboardDataInternalAsync(IClipboardDataModel data)
        {
            var dataPackage = new DataPackage();

            switch (data.DataType)
            {
                case ClipboardDataType.Text:
                    dataPackage.SetText(await data.GetDataAsync() as string);
                    break;

                default:
                    return false;
            }

            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();

            return true;
        }

        private IClipboardDataModel? GetClipboardDataInternal()
        {
            var data = Clipboard.GetContent();
            return data is not null ? new WindowsClipboardItemModel(data) : null;
        }

        private async Task<IEnumerable<IClipboardDataModel>?> GetFullClipboardDataInternalAsync()
        {
            var historyResult = await Clipboard.GetHistoryItemsAsync();
            if (historyResult.Status == ClipboardHistoryItemsResultStatus.Success)
            {
                var clipboardHistoryItems = new List<IClipboardDataModel>();
                foreach (var item in historyResult.Items)
                {
                    clipboardHistoryItems.Add(new WindowsClipboardItemModel(item.Content));
                }

                return clipboardHistoryItems;
            }

            return null;
        }
    }
}
