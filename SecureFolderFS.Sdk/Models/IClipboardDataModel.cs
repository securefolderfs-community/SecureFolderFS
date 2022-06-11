using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents clipboard item data.
    /// </summary>
    public interface IClipboardDataModel
    {
        /// <summary>
        /// Gets the type of the clipboard data.
        /// </summary>
        ClipboardDataType DataType { get; }

        /// <summary>
        /// Gets the item data.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful returns object that represents item data, otherwise null.</returns>
        Task<object?> GetDataAsync();
    }
}
