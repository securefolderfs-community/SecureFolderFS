using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SecureFolderFS.AvaloniaUI.AppModels;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
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
        public async Task<bool> SetClipboardDataAsync(IClipboardItemModel data)
        {
            switch (data.DataType)
            {
                case ClipboardDataType.Text:
                    await TextCopy.ClipboardService.SetTextAsync((await data.GetDataAsync() as string)!);
                    break;

                default:
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<IClipboardItemModel?> RequestClipboardDataAsync()
        {
            var data = await TextCopy.ClipboardService.GetTextAsync();
            return data is not null ? new ClipboardItemModel(data) : null;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IClipboardItemModel>> RequestFullClipboardDataAsync()
        {
            var data = await RequestClipboardDataAsync();
            return data is null ? Enumerable.Empty<IClipboardItemModel>() : new[] { data };
        }
    }
}