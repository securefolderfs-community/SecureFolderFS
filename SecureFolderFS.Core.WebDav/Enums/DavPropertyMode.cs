using System;

namespace SecureFolderFS.Core.WebDav.Enums
{
    [Flags]
    internal enum DavPropertyMode : uint
    {
        None = 0,
        PropertyNames = 1,
        AllProperties = 2,
        SelectedProperties = 4
    }
}
