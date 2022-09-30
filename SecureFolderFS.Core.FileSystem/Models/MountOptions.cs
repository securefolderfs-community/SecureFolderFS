using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Models
{
    public sealed class MountOptions
    {
        public string MountPoint { get; }

        public MountOptions(string mountPoint)
        {
            MountPoint = mountPoint;
        }
    }
}
