using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Shared.Helpers
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
