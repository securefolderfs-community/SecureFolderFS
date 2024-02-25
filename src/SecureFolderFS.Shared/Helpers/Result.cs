using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Shared.Helpers
{
    /// <inheritdoc cref="IResult"/>
    public class Result : IResult
    {
        public static Result Success { get; } = new(true);

        /// <inheritdoc/>
        public bool Successful { get; }

        /// <inheritdoc/>
        public Exception? Exception { get; }

        protected Result(Exception? exception)
            : this(false)
        {
            Exception = exception;
        }

        protected Result(bool isSuccess)
        {
            Successful = isSuccess;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Exception?.ToString() ?? (Successful ? "Success" : "Unsuccessful");
        }

        /// <summary>
        /// Creates a new <see cref="Result"/> with an exception.
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception"/> instance of the operation.</param>
        /// <returns>A new failed <see cref="Result"/>.</returns>
        public static Result Failure(Exception? exception) => new(exception);
    }

    /// <inheritdoc cref="IResult{T}"/>
    public class Result<T> : Result, IResult<T>
    {
        /// <inheritdoc/>
        public T? Value { get; }

        protected Result(T value, bool isSuccess = true)
            : base(isSuccess)
        {
            Value = value;
        }

        protected Result(Exception? exception)
            : base(exception)
        {
        }

        /// <summary>
        /// Creates a new <see cref="Result{T}"/> with a value.
        /// </summary>
        /// <param name="value">The vault of the result.</param>
        /// <returns>A new successful <see cref="Result{T}"/>.</returns>
        public new static Result<T> Success(T value) => new(value);

        /// <summary>
        /// Creates a new <see cref="Result{T}"/> with an exception.
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception"/> instance of the operation.</param>
        /// <returns>A new failed <see cref="Result{T}"/>.</returns>
        public new static Result<T> Failure(Exception? exception) => new(exception);

        /// <summary>
        /// Creates a new <see cref="Result{T}"/> without an exception.
        /// </summary>
        /// <param name="value">The vault of the result.</param>
        /// <returns>A new failed <see cref="Result{T}"/>.</returns>
        public static Result<T> Failure(T value) => new(value);
    }
}
