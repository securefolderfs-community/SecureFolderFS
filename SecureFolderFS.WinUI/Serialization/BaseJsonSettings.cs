using System.Runtime.CompilerServices;

#nullable enable

namespace SecureFolderFS.WinUI.Serialization
{
    internal abstract class BaseJsonSettings
    {
        public bool IsAvailable { get; private set; }

        protected ISettingsSerializer? SettingsSerializer { get; set; }

        protected IJsonSettingsSerializer? JsonSettingsSerializer { get; set; }

        protected IJsonSettingsDatabase? JsonSettingsDatabase { get; set; }

        protected void Initialize(string filePath)
        {
            IsAvailable = SettingsSerializer?.CreateFile(filePath) ?? false;
        }

        protected TValue? Get<TValue>(TValue? defaultValue, [CallerMemberName] string propertyName = "")
            where TValue : class
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return defaultValue;
            }

            return JsonSettingsDatabase?.GetValue(propertyName, defaultValue) ?? defaultValue;
        }

        protected bool Set<TValue>(TValue? value, [CallerMemberName] string propertyName = "")
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return false;
            }

            if (JsonSettingsDatabase?.SetValue(propertyName, value) ?? false)
            {
                // TODO: Notify of setting change
                return true;
            }

            return false;
        }
    }
}
