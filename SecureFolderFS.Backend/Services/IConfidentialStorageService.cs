using SecureFolderFS.Backend.Models;

namespace SecureFolderFS.Backend.Services
{
    /// <summary>
    /// Contains properties to be stored confidentially.
    /// This interface does not guarantee security of data and therefore shouldn't be used to store secrets.
    /// </summary>
    public interface IConfidentialStorageService
    {
        bool IsAvailable { get; }

        Dictionary<VaultIdModel, VaultModel> SavedVaultModels { get; set; }
    }
}
