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
        public string DisplayName { get; }

        public LanguageViewModel(CultureInfo cultureInfo)
        {
            CultureInfo = cultureInfo;
            DisplayName = FormatName(cultureInfo);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return DisplayName;
        }

        private static string FormatName(CultureInfo cultureInfo)
        {
            var name = cultureInfo.NativeName;

            // Convert the first letter to uppercase
            name = char.ToUpperInvariant(name[0]) + name.Substring(1);

            // Sometimes the name may not have the country
            if (name.Contains('('))
                return name;

            // Get the region to use for the country name
            var regionInfo = new RegionInfo(cultureInfo.Name);

            return $"{name} ({regionInfo.DisplayName})";
        }
    }
}
