using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Shared.Utilities;
using System;

namespace SecureFolderFS.Sdk.AppModels
{
    public sealed class AuthenticationModel
    {
        public string AuthenticationId { get; }

        public string AuthenticationName { get; }

        public AuthenticationType AuthenticationType { get; }

        public IAuthenticator<IDisposable>? Authenticator { get; }

        public AuthenticationModel(string authenticationId, string authenticationName, AuthenticationType authenticationType, IAuthenticator<IDisposable>? authenticator)
        {
            AuthenticationId = authenticationId;
            AuthenticationName = authenticationName;
            AuthenticationType = authenticationType;
            Authenticator = authenticator;
        }
    }
}
