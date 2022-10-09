using System;

namespace SecureFolderFS.Shared.Utils
{
    /// <summary>
    /// Represents a result of an action.
    /// </summary>
    public interface IResult
    {
        /// <summary>
        /// Gets the value that determines whether the action completed successfully.
        /// </summary>
        bool Successful { get; }

        /// <summary>
        /// Gets the exception associated with the action if any.
        /// </summary>
        Exception? Exception { get; }
    }

    /// <summary>
    /// Represents a result of an action with data.
    /// </summary>
    /// <typeparam name="T">The type of data associated with the result.</typeparam>
    public interface IResult<out T> : IResult
    {
        /// <summary>
        /// Gets the value associated with the result.
        /// </summary>
        T? Value { get; }
    }

    /// <inheritdoc cref="IResult"/>
    public interface IResultWithMessage : IResult
    {
        /// <summary>
        /// Gets formatted display message describing result of the action.
        /// </summary>
        string? Message { get; }
    }

    /// <inheritdoc cref="IResult{T}"/>
    public interface IResultWithMessage<out T> : IResultWithMessage, IResult<T>
    {
    }
}
