using FileProvider;
using Foundation;

namespace SecureFolderFS.Core.MobileFS.FileSystem
{
    public class FileProviderEnumerator : NSObject, INSFileProviderEnumerator
    {
        public void Invalidate()
        {
            throw new NotImplementedException();
        }

        public void EnumerateItems(INSFileProviderEnumerationObserver observer, NSData startPage)
        {
            throw new NotImplementedException();
        }
    }
}
