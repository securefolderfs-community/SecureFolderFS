using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Tmds.Linux.LibC;
using Tmds.Linux;

namespace Tmds.Fuse.Tests
{
    public class FuseOperationsTests : IDisposable
    {
        class FileSystem : FuseFileSystemBase
        {
            public override int GetAttr(ReadOnlySpan<byte> path, ref stat stat, FuseFileInfoRef fiRef)
            {
                if (path.SequenceEqual(Encoding.UTF8.GetBytes("/GetAttr_file1")))
                {
                    stat.st_atim = new DateTime(2000, 12, 1, 23, 13, 59, 200, DateTimeKind.Utc).ToTimespec();
                    stat.st_mtim = new DateTime(2001, 11, 2, 22, 12, 58, 199, DateTimeKind.Utc).ToTimespec();
                    stat.st_ctim = new DateTime(2002, 10, 3, 21, 11, 57, 198, DateTimeKind.Utc).ToTimespec();
                    stat.st_nlink = 10;
                    stat.st_uid = 13;
                    stat.st_gid = 15;
                    stat.st_size = 200;
                    stat.st_mode = S_IFREG | 0b100_010_001;
                    return 0;
                }
                else if (path.SequenceEqual(Encoding.UTF8.GetBytes("/GetAttr_dir1")))
                {
                    stat.st_mode = S_IFDIR | 0b100_010_001;
                    return 0;
                }
                return -ENOENT;
            }
        }

        private readonly string _mountPoint;
        private readonly IFuseMount _mount;

        public FuseOperationsTests()
        {
            _mountPoint = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_mountPoint);

            _mount = Fuse.Mount(_mountPoint, new FileSystem());
        }

        public void Dispose()
        {
            bool unmounted = _mount.UnmountAsync(1000).GetAwaiter().GetResult();
            Assert.True(unmounted);
            Directory.Delete(_mountPoint);
        }

        [Fact]
        public void GetAttr()
        {
            string filename = Path.Combine(_mountPoint, "GetAttr_file1");
            var statResult = Stat(filename);
            Assert.Equal("-r---w---x", statResult.access);
            Assert.Equal("15", statResult.group);
            Assert.Equal("10", statResult.links);
            Assert.Equal("200", statResult.size);
            Assert.Equal("13", statResult.user);
            Assert.Equal(new DateTime(2000, 12, 1, 23, 13, 59, 200, DateTimeKind.Utc), DateTime.Parse(statResult.accessTime).ToUniversalTime());
            Assert.Equal(new DateTime(2001, 11, 2, 22, 12, 58, 199, DateTimeKind.Utc), DateTime.Parse(statResult.modificationTime).ToUniversalTime());
            Assert.Equal(new DateTime(2002, 10, 3, 21, 11, 57, 198, DateTimeKind.Utc), DateTime.Parse(statResult.changeTime).ToUniversalTime());

            filename = Path.Combine(_mountPoint, "GetAttr_dir1");
            statResult = Stat(filename);
            Assert.Equal(statResult.access, "dr---w---x");
        }

        private static (string access, string group, string links, string size, string user, string accessTime, string modificationTime, string changeTime) Stat(string filename)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "stat",
                RedirectStandardOutput = true
            };
            psi.ArgumentList.Add("--printf");
            psi.ArgumentList.Add("%A\n%g\n%h\n%s\n%u\n%x\n%y\n%z");
            psi.ArgumentList.Add(filename);
            using (var process = Process.Start(psi))
            {
                string processOutput = process.StandardOutput.ReadToEnd();
                string[] split = processOutput.Split('\n');
                return (split[0], split[1], split[2], split[3], split[4], split[5], split[6], split[7]);
            }
        }
    }
}