using Microsoft.Windows.ApplicationModel.Resources;
using SecureFolderFS.Backend.Services;

#nullable enable

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    internal sealed class LocalizationService : ILocalizationService
    {
        private static readonly ResourceLoader IndependentLoader = new();

        public string GetResource(string resourceKey)
        {
            return IndependentLoader.GetString(resourceKey);
        }
    }
}
