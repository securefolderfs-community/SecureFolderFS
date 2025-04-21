using System.Collections.Generic;
using OwlCore.Storage;

namespace SecureFolderFS.Storage.Pickers
{
    /// <summary>
    /// Represents an abstract class used for specifying picker-specific filtering options.
    /// </summary>
    public abstract record PickerOptions;
    
    /// <summary>
    /// Represents a name filter.
    /// </summary>
    /// <param name="Names">The filtered names to include.</param>
    public sealed record NameFilter(IEnumerable<string> Names) : PickerOptions;

    /// <summary>
    /// Represents a starting directory picker option.
    /// </summary>
    /// <param name="Location">The suggested location where the picker should open.</param>
    public sealed record StartingFolderOptions(string Location) : PickerOptions;
}
