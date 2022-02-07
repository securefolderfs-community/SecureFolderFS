namespace SecureFolderFS.Backend.Utils
{
    /// <summary>
    /// Provides functionality for cleaning up class instances.
    /// <br/>
    /// <br/>
    /// Unlike <see cref="IDisposable"/>, using <see cref="ICleanable"/> allows for instances to be reused.
    /// </summary>
    public interface ICleanable
    {
        /// <summary>
        /// Performs the cleaning operation.
        /// </summary>
        void Cleanup();
    }
}
