using System.Globalization;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a language.
    /// </summary>
    public interface ILanguageModel
    {
        /// <summary>
        /// Gets the unique language tag associated with the culture.
        /// </summary>
        string LanguageTag { get; }

        /// <summary>
        /// Gets the name representation of the language.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the <see cref="CultureInfo"/> of the language.
        /// </summary>
        CultureInfo Culture { get; }
    }
}
