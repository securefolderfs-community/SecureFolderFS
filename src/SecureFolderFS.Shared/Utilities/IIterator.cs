using System.Collections;
using System.Collections.Generic;

namespace SecureFolderFS.Shared.Utilities
{
    /// <summary>
    /// Represents an iterator that can iterate through a collection.
    /// </summary>
    public interface IIterator : IEnumerator
    {
        /// <summary>
        /// Gets a value indicating whether the iterator has a next element.
        /// </summary>
        bool HasNext { get; }
    }

    /// <summary>
    /// Represents a generic iterator that can iterate through a collection.
    /// </summary>
    /// <typeparam name="T">The type of the element.</typeparam>
    public interface IIterator<out T> : IEnumerator<T>, IIterator
    {
    }
}
