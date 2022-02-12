using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.ViewModels;

namespace SecureFolderFS.Backend.Services
{
    public interface ISettingsService
    {
        bool IsAvailable { get; }

        Dictionary<VaultIdModel, VaultViewModel> SavedVaults { get; set; }
    }
}
