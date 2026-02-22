using SecureFolderFS.Shared.Models;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Extensions
{
    public static class AuthenticationExtensions
    {
        public static async Task<IResult<IKeyBytes>> TryEnrollAsync(this IAuthenticator authenticator,
            string id, byte[]? data, CancellationToken cancellationToken)
        {
            try
            {
                return await authenticator.EnrollAsync(id, data, cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<IKeyBytes>.Failure(ex);
            }
        }

        public static async Task<IResult<IKeyBytes>> TryAcquireAsync(this IAuthenticator authenticator,
            string id, byte[]? data, CancellationToken cancellationToken)
        {
            try
            {
                return await authenticator.AcquireAsync(id, data, cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<IKeyBytes>.Failure(ex);
            }
        }
    }
}
