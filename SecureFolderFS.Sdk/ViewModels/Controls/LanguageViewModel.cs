using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    /// <summary>
    /// Represents a language view model.
    /// </summary>
    public sealed class LanguageViewModel : ObservableObject
    {
        /// <summary>
        /// Gets the <see cref="CultureInfo"/> of the language.
        /// </summary>
        public CultureInfo CultureInfo { get; }

        /// <summary>
        /// Gets the user-friendly name representation of the language.
        /// </summary>
        public string FriendlyName { get; }

        public LanguageViewModel(CultureInfo cultureInfo)
        {
            CultureInfo = cultureInfo;
            FriendlyName = FormatName(cultureInfo);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return FriendlyName;
        }

        private static string FormatName(CultureInfo cultureInfo)
        {
            // Sometimes the name may not have the country
            var name = cultureInfo.NativeName;
            if (name.Contains('('))
                return name;

            // Convert the first letter to uppercase
            name = char.ToUpperInvariant(name[0]) + name.Substring(1);

            // Get the region to use for the country name
            var regionInfo = new RegionInfo(cultureInfo.Name);

            return $"{name} ({regionInfo.DisplayName})";
        }
    }
}
