namespace SecureFolderFS.Shared.ComponentModel
{
    /// <inheritdoc cref="IWrapper{T}"/>
    public sealed class Wrapper<T>(T inner) : IWrapper<T>
    {
        /// <inheritdoc/>
        public T Inner { get; } = inner;
    }
}
