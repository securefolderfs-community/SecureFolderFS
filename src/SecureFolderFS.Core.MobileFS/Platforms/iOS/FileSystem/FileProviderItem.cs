using FileProvider;
using Foundation;

namespace SecureFolderFS.Core.MobileFS.FileSystem
{
    public sealed class FileProviderItem : NSObject, INSFileProviderItem
    {
        public FileProviderItem(string filename)
        {
            Filename = filename;
        }

        public string Filename { get; }
    }
}
