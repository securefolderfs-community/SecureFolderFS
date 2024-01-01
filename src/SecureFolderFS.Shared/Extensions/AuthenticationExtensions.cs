using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Extensions
{
    public static class AuthenticationExtensions
    {
        public static async Task<IResult<IKey>> TryCreateAsync(this IAuthenticator authenticator,
            string id, byte[]? data, CancellationToken cancellationToken)
        {
            try
            {
                var key = await authenticator.CreateAsync(id, data, cancellationToken);
                return CommonResult<IKey>.Success(key);
            }
            catch (Exception ex)
            {
                return CommonResult<IKey>.Failure(ex);
            }
        }

        public static async Task<IResult<IKey>> TrySignAsync(this IAuthenticator authenticator,
            string id, byte[]? data, CancellationToken cancellationToken)
        {
            try
            {
                var key = await authenticator.SignAsync(id, data, cancellationToken);
                return CommonResult<IKey>.Success(key);
            }
            catch (Exception ex)
            {
                return CommonResult<IKey>.Failure(ex);
            }
        }
    }
}
