using NWebDav.Server.Storage;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage
{
    /// <summary>
    /// Allows instantiations of WebDav storage objects that wrap an implementation.
    /// </summary>
    internal interface IInstantiableDavStorage
    {
        /// <summary>
        /// Creates an instance of <see cref="IDavFile"/>.
        /// </summary>
        /// <typeparam name="T">The capability for the implementor.</typeparam>
        /// <param name="inner">The inner object that is wrapped by the implementation.</param>
        /// <returns>A new instance of <see cref="IDavFile"/> wrapping <paramref name="inner"/>.</returns>
        IDavFile NewFile<T>(T inner) where T : IFile;

        /// <summary>
        /// Creates an instance of <see cref="IDavFolder"/>.
        /// </summary>
        /// <typeparam name="T">The capability for the implementor.</typeparam>
        /// <param name="inner">The inner object that is wrapped by the implementation.</param>
        /// <returns>A new instance of <see cref="IDavFolder"/> wrapping <paramref name="inner"/>.</returns>
        IDavFolder NewFolder<T>(T inner) where T : IFolder;
    }
}
