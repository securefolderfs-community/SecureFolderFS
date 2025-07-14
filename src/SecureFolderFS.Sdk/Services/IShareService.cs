using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    public interface IShareService
    {
        Task ShareTextAsync(string text, string title);
    }
}
