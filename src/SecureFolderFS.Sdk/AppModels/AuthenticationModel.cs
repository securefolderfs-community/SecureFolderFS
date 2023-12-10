using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Shared.Utilities;

namespace SecureFolderFS.Sdk.AppModels
{
    public sealed class AuthenticationModel
    {
        public string Id { get; }

        public string Name { get; }

        public AuthenticationType AuthenticationType { get; }

        public IAuthenticator? Authenticator { get; }

        public AuthenticationModel(string id, string name, AuthenticationType authenticationType, IAuthenticator? authenticator)
        {
            Id = id;
            Name = name;
            AuthenticationType = authenticationType;
            Authenticator = authenticator;
        }
    }
}
