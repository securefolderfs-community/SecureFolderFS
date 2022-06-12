using System;

namespace SecureFolderFS.Sdk.Services.Settings
{
    public interface IApplicationSettingsService : IBaseSettingsService
    {
        DateTime UpdateLastChecked { get; set; }
    }
}
