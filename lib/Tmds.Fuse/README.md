# Tmds.Fuse

Write a Linux file system in .NET.

## Supported platforms

Tmds.Fuse supports Linux x64 platforms with libfuse 3.1+.

## Getting Started

To use a daily build, add the myget NuGet feed to `NuGet.Config`.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="tmds" value="https://www.myget.org/F/tmds/api/v3/index.json" />
  </packageSources>
</configuration>
```

Add a package reference to your project.

```sh
dotnet add package Tmds.Fuse --version '0.1.0-*'
```

Implement a simple file system:
```C#
using System;
using System.Text;
using Tmds.Fuse;
using static Tmds.Linux.LibC;

class HelloFileSystem : FuseFileSystemBase
{
    private static readonly byte[] _helloFilePath = Encoding.UTF8.GetBytes("/hello");
    private static readonly byte[] _helloFileContent = Encoding.UTF8.GetBytes("hello world!");

    public override int GetAttr(ReadOnlySpan<byte> path, ref stat stat, FuseFileInfoRef fiRef)
    {
        if (path.SequenceEqual(FuseConstants.RootPath))
        {
            stat.st_mode = S_IFDIR | 0b111_101_101; // rwxr-xr-x
            stat.st_nlink = 2; // 2 + nr of subdirectories
            return 0;
        }
        else if (path.SequenceEqual(_helloFilePath))
        {
            stat.st_mode = S_IFREG | 0b100_100_100; // r--r--r--
            stat.st_nlink = 1;
            stat.st_size = _helloFileContent.Length;
            return 0;
        }
        else
        {
            return -ENOENT;
        }
    }

    public override int Open(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
    {
        if (!path.SequenceEqual(_helloFilePath))
        {
            return -ENOENT;
        }

        if ((fi.flags & O_ACCMODE) != O_RDONLY)
        {
            return -EACCES;
        }

        return 0;
    }

    public override int Read(ReadOnlySpan<byte> path, ulong offset, Span<byte> buffer, ref FuseFileInfo fi)
    {
        if (offset > (ulong)_helloFileContent.Length)
        {
            return 0;
        }
        int intOffset = (int)offset;
        int length = (int)Math.Min(_helloFileContent.Length - intOffset, buffer.Length);
        _helloFileContent.AsSpan().Slice(intOffset, length).CopyTo(buffer);
        return length;
    }

    public override int ReadDir(ReadOnlySpan<byte> path, ulong offset, ReadDirFlags flags, DirectoryContent content, ref FuseFileInfo fi)
    {
        if (!path.SequenceEqual(FuseConstants.RootPath))
        {
            return -ENOENT;
        }
        content.AddEntry(".");
        content.AddEntry("..");
        content.AddEntry("hello");
        return 0;
    }
}
```

Implement the `Main` method:

```C#
static async Task Main(string[] args)
{
    if (!Fuse.CheckDependencies())
    {
        Console.WriteLine(Fuse.InstallationInstructions);
        return;
    }
    using (var mount = Fuse.Mount("/tmp/mountpoint", new HelloFileSystem()))
    {
        await mount.WaitForUnmountAsync();
    }
}
```

Create the mountpoint directory:
```sh
$ mkdir /tmp/mountpoint
```

Run the program:
```sh
$ dotnet run
```
