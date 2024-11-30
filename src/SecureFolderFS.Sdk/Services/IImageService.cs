using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.Services
{
    public interface IImageService
    {
        /// <summary>
        /// Gets an image representing the health state of a vault.
        /// </summary>
        /// <param name="file">The <see cref="SeverityType"/> to get the icon for.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the appropriate icon for the health state.</returns>
        Task<IImage> ReadImageFileAsync(IFile file, CancellationToken cancellationToken);
    }
}
