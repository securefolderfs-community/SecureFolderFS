using System.Threading.Tasks;

namespace SecureFolderFS.Shared.ComponentModel
{
    public interface INavigator
    {
        /// <summary>
        /// Navigates to a given <paramref name="view"/>.
        /// </summary>
        /// <param name="view">The target to navigate to.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns true; otherwise false.</returns>
        Task<bool> NavigateAsync(IViewDesignation view);

        /// <summary>
        /// Tries to navigate to the previous target, if possible.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the navigation target was present, returns true; otherwise false.</returns>
        Task<bool> GoBackAsync();

        /// <summary>
        /// Tries to navigate to the next target, if possible.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the navigation target was present, returns true; otherwise false.</returns>
        Task<bool> GoForwardAsync();
    }
}
