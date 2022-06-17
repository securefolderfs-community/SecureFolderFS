using System;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.Dispatching
{
    /// <summary>
    /// Represents a thread dispatcher. 
    /// </summary>
    public interface IThreadDispatcher
    {
        /// <summary>
        /// Determines whether the current dispatcher has thread access.
        /// </summary>
        bool HasThreadAccess { get; }

        /// <summary>
        /// Dispatches an action to run on different thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns></returns>
        Task DispatchAsync(Action action);
    }
}
