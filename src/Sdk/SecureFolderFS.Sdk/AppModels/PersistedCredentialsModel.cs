using System.Collections.Generic;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.AppModels
{
    public class PersistedCredentialsModel
    {
        public static PersistedCredentialsModel Instance { get; } = new();

        /// <summary>
        /// Gets the dictionary of credentials associated with a vault ID.
        /// </summary>
        public Dictionary<string, IKeyUsage> Credentials { get; } = new();
    }
}