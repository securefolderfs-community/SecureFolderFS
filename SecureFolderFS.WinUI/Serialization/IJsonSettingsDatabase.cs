#nullable enable

using System;

namespace SecureFolderFS.WinUI.Serialization
{
    internal interface IJsonSettingsDatabase
    {
        TValue? GetValue<TValue>(string key, Func<TValue?>? defaultValue = null);

        bool SetValue<TValue>(string key, TValue? newValue);

        bool RemoveKey(string key);

        bool FlushSettings();

        bool ImportSettings(object? import);

        object? ExportSettings();
    }
}
