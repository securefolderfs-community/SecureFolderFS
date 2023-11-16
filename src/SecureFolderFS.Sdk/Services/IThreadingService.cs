﻿using SecureFolderFS.Shared.Utilities;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service to manage current thread execution.
    /// </summary>
    public interface IThreadingService
    {
        /// <summary>
        /// Changes current thread to execute on UI thread.
        /// </summary>
        /// <returns>An <see cref="IAwaitable"/> that represents the asynchronous operation.</returns>
        IAwaitable ChangeThreadAsync();
    }
}
