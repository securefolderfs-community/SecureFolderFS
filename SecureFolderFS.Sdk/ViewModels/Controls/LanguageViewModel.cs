using CommunityToolkit.Mvvm.ComponentModel;
using System;
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
            FriendlyName = FormatName(CultureInfo.NativeName);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return FriendlyName;
        }

        private static string FormatName(string unformatted)
        {
            return string.Concat(unformatted[0].ToString().ToUpperInvariant(), unformatted.AsSpan(1));
        }
    }
}
