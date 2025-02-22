using System.Collections.Generic;

namespace SecureFolderFS.Storage.Pickers
{
    /// <summary>
    /// Represents an abstract class used for specifying picker-specific filtering options.
    /// </summary>
    public abstract record FilterOptions;
    
    /// <summary>
    /// Represents a name filter.
    /// </summary>
    /// <param name="Names">The filtered names to include.</param>
    public sealed record NameFilter(IEnumerable<string> Names) : FilterOptions;
}
