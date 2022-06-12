namespace SecureFolderFS.Shared.Utils
{
    /// <summary>
    /// Provides method to determine whether an instance contains specific value.
    /// </summary>
    /// <typeparam name="T">The type of objects to compare.</typeparam>
    public interface IContainable<in T>
    {
        bool Contains(T? other);
    }
}
