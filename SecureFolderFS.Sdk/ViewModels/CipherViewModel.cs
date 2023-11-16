using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels
{
    /// <summary>
    /// Represents a cryptographic cipher descriptor.
    /// </summary>
    public sealed class CipherViewModel : ObservableObject
    {
        /// <summary>
        /// Gets an unique id associated with this cipher.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the name of this cipher.
        /// </summary>
        public string Name { get; }

        public CipherViewModel(string id, string? name = null)
        {
            Id = id;
            Name = name ?? id;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }
}
