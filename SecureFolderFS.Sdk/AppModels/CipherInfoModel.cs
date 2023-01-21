namespace SecureFolderFS.Sdk.AppModels
{
    /// <summary>
    /// Represents a cryptographic cipher descriptor.
    /// </summary>
    /// <param name="Name">Gets the name of this cipher.</param>
    /// <param name="Id">Gets an unique id associated with this cipher.</param>
    public record class CipherInfoModel(string Name, string Id);
}
