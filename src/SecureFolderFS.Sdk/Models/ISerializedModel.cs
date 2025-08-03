namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a model that holds serialized data.
    /// </summary>
    public interface ISerializedModel
    {
        /// <summary>
        /// Retrieves the deserialized data.
        /// </summary>
        /// <returns>The deserialized data. If the value cannot be deserialized, returns null.</returns>
        TValue? GetValue<TValue>();
    }
}
