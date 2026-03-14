using AndroidX.DocumentFile.Provider;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Maui.Platforms.Android.Storage.StorageProperties
{
    /// <inheritdoc cref="ISizeOfProperty"/>
    internal sealed class AndroidSizeOfProperty : ISizeOfProperty
    {
        private readonly DocumentFile _document;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public AndroidSizeOfProperty(string id, DocumentFile document)
        {
            _document = document;
            Name = nameof(ISizeOf.SizeOf);
            Id = $"{id}/{nameof(ISizeOf.SizeOf)}";
        }

        /// <inheritdoc/>
        public Task<long?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<long?>(_document.Length());
        }
    }
}
