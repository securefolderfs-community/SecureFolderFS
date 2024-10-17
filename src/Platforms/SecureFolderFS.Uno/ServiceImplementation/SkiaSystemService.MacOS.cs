using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    internal sealed partial class SkiaSystemService
    {
        [DllImport("/System/Library/Frameworks/CoreServices.framework/CoreServices")]
        file private static extern IntPtr CGSessionCopyCurrentDictionary();

        partial void AttachEvent(ref EventHandler? handler, EventHandler? value)
        {
            UiKi
        }

        partial void DetachEvent(ref EventHandler? handler, EventHandler? value)
        {

        }
    }
}
