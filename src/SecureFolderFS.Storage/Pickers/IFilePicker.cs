using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Storage.Pickers
{
    /// <summary>
    /// Represents an abstracted interface that prompts the user to pick a file.
    /// </summary>
    public interface IFilePicker
    {
        /// <summary>
        /// Awaits the user input and picks single file.
        /// </summary>
        /// <param name="options">The filter to apply when picking files.</param>
        /// <param name="offerPersistence">Determines whether to offer persistent access to the picked item or not.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful and a file has been picked, returns <see cref="IFile"/>; otherwise null.</returns>
        Task<IFile?> PickFileAsync(PickerOptions? options, bool offerPersistence = true, CancellationToken cancellationToken = default);
    }
}
