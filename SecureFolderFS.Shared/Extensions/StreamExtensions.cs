using System.IO;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Extensions
{
    public static class StreamExtensions
    {
        public static async Task<byte[]> ReadAllBytesAsync(this Stream stream)
        {
            if (stream is MemoryStream memoryStreamImpl)
                return memoryStreamImpl.ToArray();

            await using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream.ToArray();
        }
    }
}
