using System.Threading.Tasks;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.AvaloniaUI.AppModels
{
    /// <inheritdoc cref="IClipboardItemModel"/>
    internal sealed class ClipboardItemModel : IClipboardItemModel
    {
        private readonly object? _data;

        public ClipboardItemModel(object? data)
        {
            _data = data;
            DataType = GetDataType(data);
        }

        /// <inheritdoc/>
        public ClipboardDataType DataType { get; }

        public Task<object?> GetDataAsync()
        {
            return Task.FromResult(_data);
        }

        private static ClipboardDataType GetDataType(object? data)
        {
            return data is string ? ClipboardDataType.Text : ClipboardDataType.Unknown;
        }
    }
}