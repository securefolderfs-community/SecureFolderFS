using SecureFolderFS.Core.WebDav.Storage;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage
{
    /// <summary>
    /// Allows instantiations of WebDav storage objects that wrap an implementation.
    /// </summary>
    internal interface IInstantiableDavStorage
    {
        /// <summary>
        /// Creates an instance of <see cref="DavFile{T}"/>.
        /// </summary>
        /// <typeparam name="T">The capability for the implementor.</typeparam>
        /// <param name="inner">The inner object that is wrapped by the implementation.</param>
        /// <returns>A new instance of <see cref="DavFile{T}"/> wrapping <paramref name="inner"/>.</returns>
        DavFile<T> NewFile<T>(T inner) where T : IFile;

        /// <summary>
        /// Creates an instance of <see cref="DavFolder{T}"/>.
        /// </summary>
        /// <typeparam name="T">The capability for the implementor.</typeparam>
        /// <param name="inner">The inner object that is wrapped by the implementation.</param>
        /// <returns>A new instance of <see cref="DavFolder{T}"/> wrapping <paramref name="inner"/>.</returns>
        DavFolder<T> NewFolder<T>(T inner) where T : IFolder;
    }
}
