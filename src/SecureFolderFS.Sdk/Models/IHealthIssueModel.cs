using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    public interface IHealthIssueModel
    {
        IStorable Affected { get; }

        Task TryResolveAsync(CancellationToken cancellationToken);
    }
}
