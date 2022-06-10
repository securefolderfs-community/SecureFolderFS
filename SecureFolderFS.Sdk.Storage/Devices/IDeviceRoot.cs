using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage.Devices
{
    /// <summary>
    /// Represents the root file system of any device.
    /// </summary>
    public interface IDeviceRoot
    {
        /// <summary>
        /// Tries to get an unknown root of the device.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns <see cref="object"/> that represents device root, otherwise null.</returns>
        Task<object?> GetUnknownRootAsync();

        /// <summary>
        /// Tries to get root of the device as collection of possible storage objects.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns <see cref="IEnumerable{T}"/> of type <see cref="IBaseStorage"/>, otherwise null.</returns>
        Task<IEnumerable<IBaseStorage>?> DangerousGetRootAsync();
    }
}
