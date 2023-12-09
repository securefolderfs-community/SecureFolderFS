using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Extensions
{
    public static class AuthenticationExtensions
    {
        public static async Task<IResult<TAuthentication>> TryCreateAsync<TAuthentication>(this IAuthenticator<TAuthentication> authenticator,
            string id, CancellationToken cancellationToken)
        {
            try
            {
                var authentication = await authenticator.CreateAsync(id, cancellationToken);
                return CommonResult<TAuthentication>.Success(authentication);
            }
            catch (Exception ex)
            {
                return CommonResult<TAuthentication>.Failure(ex);
            }
        }

        public static async Task<IResult<TAuthentication>> TryAuthenticateAsync<TAuthentication>(this IAuthenticator<TAuthentication> authenticator,
            string id, CancellationToken cancellationToken)
        {
            try
            {
                var authentication = await authenticator.AuthenticateAsync(id, cancellationToken);
                return CommonResult<TAuthentication>.Success(authentication);
            }
            catch (Exception ex)
            {
                return CommonResult<TAuthentication>.Failure(ex);
            }
        }
    }
}
