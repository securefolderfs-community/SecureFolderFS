using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Core.Routines
{
    public interface IOptionsRoutine : IFinalizationRoutine
    {
        void SetOptions(VaultOptions vaultOptions);
    }
}
