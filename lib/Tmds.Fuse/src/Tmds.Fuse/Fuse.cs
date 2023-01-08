using System;
using System.Diagnostics;
using System.IO;

namespace Tmds.Fuse
{
    public static class Fuse
    {
        internal const string Fusermount = "fusermount3";

        public static IFuseMount Mount(string mountPoint, IFuseFileSystem fileSystem, MountOptions options = null)
        {
            if (options == null)
            {
                options = new MountOptions();
            }

            FuseMount mount = new FuseMount(mountPoint, fileSystem, options);
            mount.Mount();
            return mount;
        }

        public static bool CheckDependencies()
            => LibFuse.IsAvailable && HasFusermount;

        public static string InstallationInstructions
        {
            get
            {
                string instructions = $"To run this library, libfuse ({LibFuse.LibraryName}) and {Fusermount} need to be installed.";
                instructions += "\nTo install these dependencies on Fedora run:";
                instructions += "\n sudo dnf install -y fuse3 fuse3-libs";
                return instructions;
            }
        }

        public static void LazyUnmount(string mountPoint)
        {
            // we need root to unmount
            // fusermount runs as root (setuid)
            var psi = new ProcessStartInfo
            {
                FileName = Fusermount,
                Arguments = $"-u -q -z {mountPoint}",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using (var process = Process.Start(psi))
            {
                process.WaitForExit();
            }
        }

        private static bool HasFusermount => HasProgramOnPath(Fusermount);

        private static bool HasProgramOnPath(string program)
        {
            string pathEnvVar = Environment.GetEnvironmentVariable("PATH");
            if (pathEnvVar != null)
            {
                var segments = pathEnvVar.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var subPath in segments)
                {
                    string path = Path.Combine(subPath, program);
                    if (File.Exists(path))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}