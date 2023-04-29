using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels
{
    /// <summary>
    /// Represents a cryptographic cipher descriptor.
    /// </summary>
    public sealed class CipherInfoViewModel : ObservableObject
    {
        /// <summary>
        /// Gets an unique id associated with this cipher.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the name of this cipher.
        /// </summary>
        public string Name { get; }

        public CipherInfoViewModel(string id, string? name = null)
        {
            Id = id;
            Name = name ?? id;
        }
    }
}
