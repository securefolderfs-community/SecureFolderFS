using SecureFolderFS.Shared.Utils;
using System;

namespace SecureFolderFS.Shared.Helpers
{
    /// <inheritdoc cref="IResult"/>
    public class CommonResult : IResult
    {
        /// <inheritdoc/>
        public bool Successful { get; }

        /// <inheritdoc/>
        public Exception? Exception { get; }

        public CommonResult(bool isSuccess = true)
        {
            Successful = isSuccess;
        }

        public CommonResult(Exception? exception)
        {
            Exception = exception;
            Successful = false;
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
