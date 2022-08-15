using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.WinUI.AppModels
{
    /// <inheritdoc cref="ICipherInfoModel"/>
    internal sealed class CipherDescriptor : ICipherInfoModel
    {
        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string Id { get; }

        public CipherDescriptor(string name, string id)
        {
            Name = name;
            Id = id;
        }

        /// <inheritdoc/>
        public Task<IResult> IsSupportedAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IResult>(new CommonResult());
        }

        /// <inheritdoc/>
        public bool Equals(ICipherInfoModel? other)
        {
            if (other is null)
                return false;

            return other.Id.Equals(Id);
        }
    }
}
