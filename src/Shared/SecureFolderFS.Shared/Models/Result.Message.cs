using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Shared.Models
{
    /// <inheritdoc cref="IResultWithMessage"/>
    public class MessageResult : Result, IResultWithMessage
    {
        /// <inheritdoc/>
        public string? Message { get; }

        public MessageResult(Exception? exception, string? message = null)
            : base(exception)
        {
            Message = message;
        }

        public MessageResult(bool isSuccess, string? message = null)
            : base(isSuccess)
        {
            Message = message;
        }

        /// <summary>
        /// Creates an instance of <see cref="IResultWithMessage"/> by combining the given base result and an additional message.
        /// </summary>
        /// <param name="baseResult">The base result, which contains the success state or an exception.</param>
        /// <param name="message">The additional message to include in the result.</param>
        /// <returns>A new instance of <see cref="IResultWithMessage"/> containing the provided base result and message.</returns>
        public static IResultWithMessage WithMessage(IResult baseResult, string message)
        {
            return baseResult.Exception is not null
                ? new MessageResult(baseResult.Exception, message)
                : new MessageResult(baseResult.Successful, message);
        }
    }

    /// <inheritdoc cref="IResultWithMessage{T}"/>
    public class MessageResult<T> : Result<T>, IResultWithMessage<T>
    {
        /// <inheritdoc/>
        public string? Message { get; }

        public MessageResult(T value, string? message = null, bool isSuccess = true)
            : base(value, isSuccess)
        {
            Message = message;
        }

        public MessageResult(Exception? exception, string? message = null)
            : base(exception)
        {
            Message = message;
        }
    }
}
