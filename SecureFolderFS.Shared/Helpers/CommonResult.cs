using System;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Shared.Helpers
{
    /// <inheritdoc cref="IResult"/>
    public sealed class CommonResult : IResult
    {
        /// <inheritdoc/>
        public bool IsSuccess { get; }

        /// <inheritdoc/>
        public Exception? Exception { get; }

        public CommonResult(bool isSuccess = true)
        {
            IsSuccess = isSuccess;
        }

        public CommonResult(Exception exception)
        {
            Exception = exception;
            IsSuccess = false;
        }
    }
}
