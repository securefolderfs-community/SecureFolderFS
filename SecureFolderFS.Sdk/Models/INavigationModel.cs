using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// A model used for managing navigation targets.
    /// </summary>
    public interface INavigationModel
    {
        /// <summary>
        /// Gets the currently navigated-to target.
        /// </summary>
        INavigationTarget? CurrentTarget { get; }

        /// <summary>
        /// Navigates to a given <paramref name="target"/> and updates existing <see cref="CurrentTarget"/>.
        /// </summary>
        /// <param name="target">The target to navigate to.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task NavigateAsync(INavigationTarget target);
    }
}
