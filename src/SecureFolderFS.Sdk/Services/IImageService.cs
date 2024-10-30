using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Shared.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    public interface IImageService
    {
        /// <summary>
        /// Gets an image representing the health state of a vault.
        /// </summary>
        /// <param name="severity">The <see cref="SeverityType"/> to get the icon for.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the appropriate icon for the health state.</returns>
        Task<IImage> GetHealthIconAsync(SeverityType severity);
    }
}
