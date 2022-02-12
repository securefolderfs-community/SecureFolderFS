using Newtonsoft.Json;

namespace SecureFolderFS.Backend.Models
{
    [Serializable]
    public sealed class VaultIdModel : IEquatable<VaultIdModel>
    {
        [JsonIgnore]
        private Guid Id { get; set; }

        [JsonProperty("Id")]
        private string GuidId
        {
            get => Id.ToString();
            set => Id = Guid.Parse(value);
        }

        public VaultIdModel()
        {
            Id = Guid.NewGuid();
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool Equals(VaultIdModel? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is VaultIdModel other && Equals(other);
        }
    }
}
