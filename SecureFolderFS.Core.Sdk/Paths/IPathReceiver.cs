using System;

namespace SecureFolderFS.Core.Sdk.Paths
{
    /// <summary>
    /// Provides module for handling SecureFolderFS paths.
    /// <br/>
    /// <br/>
    /// This API is exposed.
    /// </summary>
    public interface IPathReceiver : IDisposable
    {
        /// <summary>
        /// Converts provided <paramref name="ciphertextPath"/> to implementation of <typeparamref name="TRequestedPath"/>.
        /// </summary>
        /// <typeparam name="TRequestedPath">The path type.</typeparam>
        /// <param name="ciphertextPath">The raw ciphertext path.</param>
        /// <returns>Requested <typeparamref name="TRequestedPath"/>.</returns>
        TRequestedPath FromCiphertextPath<TRequestedPath>(string ciphertextPath) where TRequestedPath : IPath;

        /// <summary>
        /// Gets active cleartext file name from provided ciphertext path.
        /// </summary>
        /// <param name="ciphertextFilePath">The path.</param>
        /// <returns></returns>
        string GetCleartextFileName(string ciphertextFilePath); // TODO: Refactor // TODO: An interface here? return- ICleartextFileName, param- ICiphertextFileNameFromRoot : ICiphertextFileName

        /// <summary>
        /// Gets active ciphertext file name from provided cleartext path.
        /// </summary>
        /// <param name="cleartextFilePath">The path.</param>
        /// <returns></returns>
        string GetCiphertextFileName(string cleartextFilePath); // TODO: Refactor

        /// <summary>
        /// Converts provided <paramref name="ciphertextPath"/> to <see cref="ICleartextPath"/>.
        /// </summary>
        /// <param name="ciphertextPath">The ciphertext path.</param>
        /// <returns>Converted cleartext version of <paramref name="ciphertextPath"/>.</returns>
        ICleartextPath FromCiphertextPath(ICiphertextPath ciphertextPath);

        /// <summary>
        /// Converts provided <paramref name="cleartextPath"/> to implementation of <typeparamref name="TRequestedPath"/>.
        /// </summary>
        /// <typeparam name="TRequestedPath">The path type.</typeparam>
        /// <param name="cleartextPath">The raw cleartext path.</param>
        /// <returns>Requested <typeparamref name="TRequestedPath"/>.</returns>
        TRequestedPath FromCleartextPath<TRequestedPath>(string cleartextPath) where TRequestedPath : IPath;

        /// <summary>
        /// Converts provided <paramref name="cleartextPath"/> to <see cref="ICiphertextPath"/>.
        /// </summary>
        /// <param name="cleartextPath">The cleartext path.</param>
        /// <returns>Converted ciphertext version of <paramref name="cleartextPath"/>.</returns>
        ICiphertextPath FromCleartextPath(ICleartextPath cleartextPath);
    }
}
