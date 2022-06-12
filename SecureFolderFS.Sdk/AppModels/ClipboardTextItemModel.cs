using System.Threading.Tasks;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IClipboardDataModel"/>
    internal sealed class ClipboardTextItemModel : IClipboardDataModel
    {
        private readonly string _text;

        /// <inheritdoc/>
        public ClipboardDataType DataType { get; } = ClipboardDataType.Text;

        public ClipboardTextItemModel(string text)
        {
            _text = text;
        }

        /// <inheritdoc/>
        public Task<object?> GetDataAsync()
        {
            return Task.FromResult<object?>(_text);
        }
    }
}
