using System.Linq;

namespace SecureFolderFS.Shared.Models
{
    /// <summary>
    /// Represents an authentication procedure wherein the <see cref="Methods"/> describe the configured authenticators.
    /// <br/>
    /// Substitutional authentication methods are represented by the <see cref="Complementation"/> member.
    /// </summary>
    /// <param name="Methods">A collection of IDs of configured authenticators.</param>
    /// <param name="Complementation">The ID of the complementing authenticator, if any.</param>
    public record class AuthenticationMethod(string[] Methods, string? Complementation)
    {
        public const char METHOD_SEPARATOR = ';';
        public const char DIFFERENTIATION_SEPARATOR = '|';

        /// <inheritdoc/>
        public override string ToString()
        {
            var primary = string.Join(METHOD_SEPARATOR, Methods);
            var complementation = string.Join(METHOD_SEPARATOR, Complementation);
            var final = string.Join(DIFFERENTIATION_SEPARATOR, primary, complementation).TrimEnd(DIFFERENTIATION_SEPARATOR);

            return final;
        }

        /// <summary>
        /// Converts a raw authentication method to <see cref="AuthenticationMethod"/>.
        /// </summary>
        /// <param name="rawMethod">The raw formatted unlock procedure string.</param>
        /// <returns>A new instance of <see cref="AuthenticationMethod"/> populated with appropriate authentication properties.</returns>
        public static AuthenticationMethod FromString(string rawMethod)
        {
            var split = rawMethod.Split(DIFFERENTIATION_SEPARATOR, 2);
            var primarySplit = split[0].Split(METHOD_SEPARATOR);
            var complementationSplit = split.ElementAtOrDefault(1)?.Split(METHOD_SEPARATOR);

            return new(primarySplit, complementationSplit?.FirstOrDefault()); // For now, only one complementation is allowed
        }
    }
}
