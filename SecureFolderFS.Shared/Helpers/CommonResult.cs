using SecureFolderFS.Shared.Utils;
using System;

namespace SecureFolderFS.Shared.Helpers
{
    /// <inheritdoc cref="IResult"/>
    public class CommonResult : IResult
    {
        public static CommonResult Success { get; } = new();

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

        public CommonResult(IResult result, Exception? exception = null)
        {
            Exception = result.Exception ?? exception;
            Successful = Exception is null;
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

        public CommonResult(IResult<T> result, T? value = default, Exception? exception = null)
            : base(result, exception)
        {
            Value = result.Value ?? value;
        }
    }
}
