using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Shared.Utilities;
using System;

namespace SecureFolderFS.Sdk.AppModels
{
    public sealed class AuthenticationModel
    {
        public string AuthenticationName { get; }

        public AuthenticationType AuthenticationType { get; }

        public IAuthenticator<IDisposable>? Authenticator { get; }

        public AuthenticationModel(string authenticationName, AuthenticationType authenticationType, IAuthenticator<IDisposable>? authenticator)
        {
            AuthenticationName = authenticationName;
            AuthenticationType = authenticationType;
            Authenticator = authenticator;
        }
    }
}
