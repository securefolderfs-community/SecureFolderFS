using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Shared.Extensions
{
    public static class RemoteResourceExtensions
    {
        /// <inheritdoc cref="IRemoteResource{T}.ConnectAsync(CancellationToken)"/>
        public static async Task<IResult<T>> TryConnectAsync<T>(this IRemoteResource<T> remoteResource, CancellationToken cancellationToken = default)
        {
            try
            {
                var value = await remoteResource.ConnectAsync(cancellationToken);
                return Result<T>.Success(value);
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(ex);
            }
        }
    }
}