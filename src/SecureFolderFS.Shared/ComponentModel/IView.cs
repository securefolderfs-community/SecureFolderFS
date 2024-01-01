using System.ComponentModel;

namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Represents a generic view.
    /// </summary>
    public interface IView : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the title of this view.
        /// </summary>
        string Title { get; }
    }
}
