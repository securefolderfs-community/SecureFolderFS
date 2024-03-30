using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Android.Provider;
using Android.Content;


using System.Threading.Tasks;
using Android.Database;
using Android.OS;

namespace SecureFolderFS.Maui.Platforms.Android.FileSystem
{
    internal sealed class AndroidFileSystemProvider : DocumentsProvider
    {
        public override bool OnCreate()
        {
            throw new NotImplementedException();
        }

        public override ParcelFileDescriptor? OpenDocument(string? documentId, string? mode, CancellationSignal? signal)
        {
            throw new NotImplementedException();
        }

        public override ICursor? QueryChildDocuments(string? parentDocumentId, string[]? projection, string? sortOrder)
        {
            throw new NotImplementedException();
        }

        public override ICursor? QueryDocument(string? documentId, string[]? projection)
        {
            throw new NotImplementedException();
        }

        public override ICursor? QueryRoots(string[]? projection)
        {
            throw new NotImplementedException();
        }
    }
}
