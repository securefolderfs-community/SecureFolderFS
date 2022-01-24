using SecureFolderFS.Core.Instance;

#nullable enable

namespace SecureFolderFS.Backend.Models
{
    public sealed class VaultModel : IEquatable<VaultModel>
    {
        public string? VaultRootPath { get; init; }

        public string? VaultName { get; init; }

        public bool Equals(VaultModel? other)
        {
            return VaultRootPath?.Equals(other?.VaultRootPath) ?? base.Equals(other);
        }

        public override int GetHashCode()
        {
            return VaultRootPath?.GetHashCode() ?? base.GetHashCode();
        }
    }
}
