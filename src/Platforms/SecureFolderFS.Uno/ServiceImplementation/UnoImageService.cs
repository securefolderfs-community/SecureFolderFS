using System.Threading.Tasks;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <inheritdoc cref="IImageService"/>
    internal sealed class UnoImageService : IImageService
    {
        /// <inheritdoc/>
        public Task<IImage> GetHealthIconAsync(VaultHealthState healthState)
        {
            return Task.FromResult<IImage>(null!); // TODO: Implement
        }
    }
}
