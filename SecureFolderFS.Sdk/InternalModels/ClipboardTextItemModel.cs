using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.InternalModels
{
    internal sealed class ClipboardTextItemModel : IClipboardDataModel
    {
        private readonly string _text;

        public ClipboardDataType DataType { get; } = ClipboardDataType.Text;

        public ClipboardTextItemModel(string text)
        {
            _text = text;
        }

        public Task<object?> GetDataAsync()
        {
            return Task.FromResult<object?>(_text);
        }
    }
}
