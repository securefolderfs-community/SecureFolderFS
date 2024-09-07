using FileProvider;
using Foundation;
using OwlCore.Storage;

namespace SecureFolderFS.Core.MobileFS.FileSystem
{
    public sealed class FileProviderItem : NSObject, INSFileProviderItem
    {
        public string Filename { get; }
        
        private FileProviderItem(string filename)
        {
            Filename = filename;
        }

        public static FileProviderItem FromFile(IFile file)
        {
            return null;
        }
    }
}
