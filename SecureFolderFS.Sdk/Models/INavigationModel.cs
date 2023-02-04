using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// A model used for managing navigation targets.
    /// </summary>
    /// <typeparam name="T">The identifier type used to associate instances of <see cref="INavigationTarget"/>.</typeparam>
    public interface INavigationModel<in T>
    {
        /// <summary>
        /// Gets the currently navigated-to target.
        /// </summary>
        INavigationTarget? CurrentTarget { get; }

        /// <summary>
        /// Tries to get an instance of <see cref="INavigationTarget"/> that is identified by <typeparamref name="T"/>.
        /// </summary>
        /// <param name="identifier">The <typeparamref name="T"/> that associates with each <see cref="INavigationTarget"/>.</param>
        /// <returns>An existing instance of <see cref="INavigationTarget"/>.</returns>
        INavigationTarget? TryGet(T identifier);

        /// <summary>
        /// Removes <paramref name="target"/> from known targets, if possible.
        /// </summary>
        /// <param name="target">A <see cref="INavigationTarget"/> to discard.</param>
        void Remove(INavigationTarget target);

        /// <summary>
        /// Tries to navigate to previous target.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the navigation target was present and navigated successfully, returns true, otherwise false.</returns>
        Task<bool> GoBackAsync();

        /// <summary>
        /// Tries to navigate to next target.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the navigation target was present and navigated successfully, returns true, otherwise false.</returns>
        Task<bool> GoForwardAsync();

        /// <summary>
        /// Navigates to a given <paramref name="target"/> and updates existing <see cref="CurrentTarget"/>.
        /// </summary>
        /// <param name="target">The target to navigate to.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task NavigateAsync(INavigationTarget target);
    }
}
