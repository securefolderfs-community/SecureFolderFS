using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.WinUI.AppModels;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IClipboardService"/>
    internal sealed class ClipboardService : IClipboardService
    {
        /// <inheritdoc/>
        public Task<bool> IsClipboardAvailableAsync()
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> SetClipboardDataAsync(IClipboardItemModel data)
        {
            return SetClipboardDataInternalAsync(data);
        }

        /// <inheritdoc/>
        public Task<IClipboardItemModel?> RequestClipboardDataAsync()
        {
            return Task.FromResult(GetClipboardDataInternal());
        }

        /// <inheritdoc/>
        public Task<IEnumerable<IClipboardItemModel>> RequestFullClipboardDataAsync()
        {
            return GetFullClipboardDataInternalAsync();
        }

        private static async Task<bool> SetClipboardDataInternalAsync(IClipboardItemModel data)
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

        private static IClipboardItemModel? GetClipboardDataInternal()
        {
            var data = Clipboard.GetContent();
            return data is not null ? new WindowsClipboardItemModel(data) : null;
        }

        private static async Task<IEnumerable<IClipboardItemModel>> GetFullClipboardDataInternalAsync()
        {
            var historyResult = await Clipboard.GetHistoryItemsAsync();
            if (historyResult.Status == ClipboardHistoryItemsResultStatus.Success)
            {
                var clipboardHistoryItems = new List<IClipboardItemModel>(historyResult.Items.Count);
                foreach (var item in historyResult.Items)
                {
                    clipboardHistoryItems.Add(new WindowsClipboardItemModel(item.Content));
                }

                return clipboardHistoryItems;
            }

            return Enumerable.Empty<IClipboardItemModel>();
        }
    }
}
