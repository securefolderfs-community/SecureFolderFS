using System.Runtime.CompilerServices;

namespace SecureFolderFS.Shared.Utils
{
    /// <summary>
    /// Represents an awaitable action.
    /// </summary>
    public interface IAwaitable : INotifyCompletion
    {
        bool IsCompleted { get; }

        IAwaitable GetAwaiter();

        void GetResult();
    }

    /// <summary>
    /// Represents an awaitable action with result.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    public interface IAwaitable<out TResult> : INotifyCompletion
    {
        bool IsCompleted { get; }

        IAwaitable<TResult> GetAwaiter();

        TResult GetResult();
    }
}
