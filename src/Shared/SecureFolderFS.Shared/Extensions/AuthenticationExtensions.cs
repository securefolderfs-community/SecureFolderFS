using SecureFolderFS.Shared.Models;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Extensions
{
    public static class AuthenticationExtensions
    {
        public static async Task<IResult<IKeyBytes>> TryCreateAsync(this IAuthenticator authenticator,
            string id, byte[]? data, CancellationToken cancellationToken)
        {
            try
            {
                var key = await authenticator.EnrollAsync(id, data, cancellationToken);
                return Result<IKeyBytes>.Success(key);
            }
            catch (Exception ex)
            {
                return Result<IKeyBytes>.Failure(ex);
            }
        }

        public static async Task<IResult<IKeyBytes>> TrySignAsync(this IAuthenticator authenticator,
            string id, byte[]? data, CancellationToken cancellationToken)
        {
            try
            {
                var key = await authenticator.AcquireAsync(id, data, cancellationToken);
                return Result<IKeyBytes>.Success(key);
            }
            catch (Exception ex)
            {
                return Result<IKeyBytes>.Failure(ex);
            }
        }
    }
}
