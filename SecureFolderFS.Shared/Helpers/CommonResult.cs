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

        public CommonResult(Exception? exception)
            : this(false)
        {
            Exception = exception;
        }

        public CommonResult(bool isSuccess = true)
        {
            Successful = isSuccess;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Exception?.ToString() ?? "Success";
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
