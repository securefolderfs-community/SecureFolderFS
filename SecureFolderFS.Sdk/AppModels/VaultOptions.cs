namespace SecureFolderFS.Sdk.AppModels
{
    public sealed class VaultOptions
    {
        public required string ContentCipherId { get; init; }

        public required string FileNameCipherId { get; init; }

        public required string Specialization { get; init; }

        public required string AuthenticationMethod { get; init; }
    }
}
