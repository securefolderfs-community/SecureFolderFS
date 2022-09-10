namespace SecureFolderFS.Core.FileSystem.Streams
{
    /// <summary>
    /// Provides access to initialization of <see cref="IStreamsManager"/>.
    /// </summary>
    public interface IStreamsManagerInitializer
    {
        /// <summary>
        /// Initializes new instance of <see cref="IStreamsManager"/>.
        /// </summary>
        /// <returns>A new instance of initialized <see cref="IStreamsManager"/>.</returns>
        IStreamsManager GetStreamsManager();
    }
}
