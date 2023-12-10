using SecureFolderFS.Shared.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace SecureFolderFS.WinUI.Authenticators
{
    /// <inheritdoc cref="IAuthenticator{TAuthentication}"/>
    public sealed class WindowsHelloAuthenticator : IAuthenticator<IDisposable>
    {
        /// <inheritdoc/>
        public async Task<IDisposable> CreateAsync(string id, CancellationToken cancellationToken)
        {
            var result = await KeyCredentialManager.RequestCreateAsync(id, KeyCredentialCreationOption.ReplaceExisting).AsTask(cancellationToken);
            if (result.Status == KeyCredentialStatus.Success)
            {
                //return result.Credential.
            }

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<IDisposable> AuthenticateAsync(string id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
