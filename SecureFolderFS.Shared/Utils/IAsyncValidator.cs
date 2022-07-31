using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Utils
{
    /// <summary>
    /// Provides data validation abstractions for <typeparamref name="T"/> data.
    /// </summary>
    /// <typeparam name="T">The type of data validated.</typeparam>
    public interface IAsyncValidator<in T>
    {
        /// <summary>
        /// Validates <see cref="value"/> and returns appropriate <see cref="IResult"/>.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns <see cref="IResult"/> based validation of the data.</returns>
        Task<IResult> ValidateAsync(T value, CancellationToken cancellationToken = default);
    }
}
