using System;

namespace SecureFolderFS.Shared.Models
{
    /// <summary>
    /// Represents a scope-based disposable object that executes a specified action upon disposal.
    /// </summary>
    public sealed class DisposableScope(Action onDispose) : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose()
        {
            onDispose();
        }
    }
}
