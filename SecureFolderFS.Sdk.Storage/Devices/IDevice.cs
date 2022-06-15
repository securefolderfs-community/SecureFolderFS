using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage.Devices
{
    /// <summary>
    /// Represents a generic device.
    /// </summary>
    public interface IDevice
    {
        /// <summary>
        /// Gets the name of the device.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Pings the device and waits for response.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the device responded and can be accessed, returns true otherwise false.</returns>
        Task<bool> PingAsync();

        /// <summary>
        /// Gets the root storage of the device.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If access is successful, returns <see cref="IDeviceRoot"/> otherwise null.</returns>
        Task<IDeviceRoot?> GetRootAsync();
    }
}
