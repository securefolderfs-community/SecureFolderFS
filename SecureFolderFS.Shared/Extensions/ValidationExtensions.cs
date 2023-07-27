using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Shared
{
    public static class ValidationExtensions
    {
        public static async Task<IResult> TryValidateAsync<T>(this IAsyncValidator<T> validator, T value, CancellationToken cancellationToken)
        {
            try
            {
                await validator.ValidateAsync(value, cancellationToken);
                return new CommonResult(true);
            }
            catch (Exception ex)
            {
                return new CommonResult(ex);
            }
        }
    }
}
