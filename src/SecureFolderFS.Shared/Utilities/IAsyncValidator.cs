﻿using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Utilities
{
    /// <summary>
    /// Provides data validation abstractions for <typeparamref name="T"/> data.
    /// </summary>
    /// <typeparam name="T">The type of data validated.</typeparam>
    public interface IAsyncValidator<in T>
    {
        /// <summary>
        /// Validates <paramref name="value"/> and returns <see cref="IResult"/> of validation.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task ValidateAsync(T value, CancellationToken cancellationToken = default);
    }
}
