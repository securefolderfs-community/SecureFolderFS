namespace SecureFolderFS.Core.Exceptions
{
    public sealed class UnauthenticVaultConfigurationException : IntegrityException
    {
        public UnauthenticVaultConfigurationException()
            : base("Vault configuration file is not authenticate.")
        {
        }
    }
}
