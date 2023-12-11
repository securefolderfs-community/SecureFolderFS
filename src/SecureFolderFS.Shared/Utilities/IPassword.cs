using System;
using System.Collections.Generic;

namespace SecureFolderFS.Shared.Utilities
{
    /// <summary>
    /// Represents a password that can be cleared.
    /// </summary>
    public interface IPassword : IKey
    {
        /// <summary>
        /// Gets the number of characters in the password.
        /// </summary>
        /// <remarks>
        /// The number of characters may not be equal to the number of bytes in the password.
        /// </remarks>
        int Length { get; }

        /// <summary>
        /// Gets the password as a sequence of characters.
        /// </summary>
        /// <remarks>
        /// By default, this method should return a UTF-8 encoded string
        /// which may vary from the originally encoded password.
        /// <br/>
        /// Prefer to use the implementing <see cref="IEnumerable{T}"/> of <see cref="byte"/>
        /// over <see cref="ToString"/> to avoid leaving copies of the password in memory.
        /// </remarks>
        /// <returns>A password as a <see cref="string"/>.</returns>
        string ToString();
    }
}
