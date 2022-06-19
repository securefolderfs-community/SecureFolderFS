﻿using System;

namespace SecureFolderFS.Sdk.Services.Settings
{
    public interface IApplicationSettingsService
    {
        bool IsAvailable { get; }

        DateTime UpdateLastChecked { get; set; }
    }
}