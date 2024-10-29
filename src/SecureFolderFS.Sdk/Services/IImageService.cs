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
        /// <param name="healthState">The <see cref="VaultHealthState"/> to get the icon for.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the appropriate icon for the health state.</returns>
        Task<IImage> GetHealthIconAsync(VaultHealthState healthState);
    }
}
