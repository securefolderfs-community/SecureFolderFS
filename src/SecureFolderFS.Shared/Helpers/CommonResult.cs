using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Shared.Helpers
{
    /// <inheritdoc cref="IResult"/>
    public class CommonResult : IResult
    {
        public static CommonResult Success { get; } = new(true);

        /// <inheritdoc/>
        public bool Successful { get; }

        /// <inheritdoc/>
        public Exception? Exception { get; }

        protected CommonResult(Exception? exception)
            : this(false)
        {
            Exception = exception;
        }

        protected CommonResult(bool isSuccess)
        {
            Successful = isSuccess;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Exception?.ToString() ?? (Successful ? "Success" : "Unsuccessful");
        }

        /// <summary>
        /// Creates a new <see cref="CommonResult{T}"/> with an exception.
        /// </summary>
        /// <param name="exception">The exception of the operation.</param>
        /// <returns>A new failed <see cref="CommonResult{T}"/>.</returns>
        public static CommonResult Failure(Exception? exception) => new(exception);
    }

    /// <inheritdoc cref="IResult{T}"/>
    public class CommonResult<T> : CommonResult, IResult<T>
    {
        /// <inheritdoc/>
        public T? Value { get; }

        protected CommonResult(T value, bool isSuccess = true)
            : base(isSuccess)
        {
            Value = value;
        }

        protected CommonResult(Exception? exception)
            : base(exception)
        {
        }

        /// <summary>
        /// Creates a new <see cref="CommonResult{T}"/> with a value.
        /// </summary>
        /// <param name="value">The vault of the result.</param>
        /// <returns>A new successful <see cref="CommonResult{T}"/>.</returns>
        public new static CommonResult<T> Success(T value) => new(value);

        /// <summary>
        /// Creates a new <see cref="CommonResult{T}"/> with an exception.
        /// </summary>
        /// <param name="exception">The exception of the operation.</param>
        /// <returns>A new failed <see cref="CommonResult{T}"/>.</returns>
        public new static CommonResult<T> Failure(Exception? exception) => new(exception);

        /// <summary>
        /// Creates a new <see cref="CommonResult{T}"/> without an exception..
        /// </summary>
        /// <param name="value">The vault of the result.</param>
        /// <returns>A new failed <see cref="CommonResult{T}"/>.</returns>
        public static CommonResult<T> Failure(T value) => new(value);
    }
}
