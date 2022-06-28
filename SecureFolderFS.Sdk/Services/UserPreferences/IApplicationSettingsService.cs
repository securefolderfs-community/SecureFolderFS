using System;

namespace SecureFolderFS.Sdk.Services.UserPreferences
{
    public interface IApplicationSettingsService
    {
        bool IsAvailable { get; }

        DateTime UpdateLastChecked { get; set; }
    }
}
