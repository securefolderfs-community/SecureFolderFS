using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Shared.Utilities;

namespace SecureFolderFS.Sdk.AppModels
{
    public sealed class AuthenticationModel
    {
        public string AuthenticationName { get; }

        public AuthenticationType AuthenticationType { get; }

        public IAuthenticator? Authenticator { get; }

        public AuthenticationModel(string authenticationName, AuthenticationType authenticationType, IAuthenticator? authenticator)
        {
            AuthenticationName = authenticationName;
            AuthenticationType = authenticationType;
            Authenticator = authenticator;
        }
    }
}
