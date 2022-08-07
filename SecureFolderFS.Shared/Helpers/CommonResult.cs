using System;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Shared.Helpers
{
    /// <inheritdoc cref="IResult"/>
    public class CommonResult : IResult
    {
        /// <inheritdoc/>
        public bool IsSuccess { get; }

        /// <inheritdoc/>
        public Exception? Exception { get; }

        public CommonResult(bool isSuccess = true)
        {
            IsSuccess = isSuccess;
        }

        public CommonResult(Exception? exception)
        {
            Exception = exception;
            IsSuccess = false;
        }
    }

    /// <inheritdoc cref="IResult{T}"/>
    public class CommonResult<T> : CommonResult, IResult<T>
    {
        /// <inheritdoc/>
        public T? Value { get; }

        public CommonResult(T value, bool isSuccess = true)
            : base(isSuccess)
        {
            Value = value;
        }

        public CommonResult(Exception? exception)
            : base(exception)
        {
        }
    }
}
