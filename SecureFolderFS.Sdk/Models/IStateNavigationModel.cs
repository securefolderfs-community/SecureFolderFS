using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// A model used for managing navigation targets and persisting state.
    /// </summary>
    public interface IStateNavigationModel : INavigationModel
    {
        /// <summary>
        /// Gets the collection of all previously navigated-to targets.
        /// </summary>
        ICollection<INavigationTarget> Targets { get; }

        /// <summary>
        /// Tries to navigate to the previous target.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the navigation target was present, returns true, otherwise false.</returns>
        Task<bool> GoBackAsync();

        /// <summary>
        /// Tries to navigate to the next target.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the navigation target was present, returns true, otherwise false.</returns>
        Task<bool> GoForwardAsync();
    }
}
