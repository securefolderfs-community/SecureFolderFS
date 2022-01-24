//using System;
//using System.IO;
//using DokanNet;

// TODO: Implement Dokan Notify once Dokany v2 is released for C#

//namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan
//{
//    internal sealed class DokanNotify : IDisposable
//    {
//        private string SourcePath { get; init; }

//        private string TargetPath { get; init; }

//        private DokanInstance DokanInstance { get; init; }

//        private FileSystemWatcher CommonFsWatcher { get; init; }

//        private FileSystemWatcher FileFsWatcher { get; init; }

//        private FileSystemWatcher DirFsWatcher { get; init; }

//        private string AlterPathToMountPath(string path)
//        {
//            var relativeMirrorPath = path.Substring(SourcePath.Length).TrimStart('\\');

//            return Path.Combine(TargetPath, relativeMirrorPath);
//        }

//        private void OnCommonFileSystemWatcher_FileDeleted(object sender, FileSystemEventArgs e)
//        {
//            var fullPath = AlterPathToMountPath(e.FullPath);

//            DokanNet.Dokan.Notify.Delete(_dokanCurrentInstance, fullPath, false);
//        }

//        private void OnCommonFileSystemWatcher_DirectoryDeleted(object sender, FileSystemEventArgs e)
//        {
//            var fullPath = AlterPathToMountPath(e.FullPath);

//            DokanNet.Dokan.Notify.Delete(_dokanCurrentInstance, fullPath, true);
//        }

//        private void OnCommonFileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
//        {
//            var fullPath = AlterPathToMountPath(e.FullPath);

//            DokanNet.Dokan.Notify.Update(_dokanCurrentInstance, fullPath);
//        }

//        private void OnCommonFileSystemWatcher_Created(object sender, FileSystemEventArgs e)
//        {
//            var fullPath = AlterPathToMountPath(e.FullPath);
//            var isDirectory = Directory.Exists(fullPath);

//            DokanNet.Dokan.Notify.Create(_dokanCurrentInstance, fullPath, isDirectory);
//        }

//        private void OnCommonFileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
//        {
//            var oldFullPath = AlterPathToMountPath(e.OldFullPath);
//            var oldDirectoryName = Path.GetDirectoryName(e.OldFullPath);

//            var fullPath = AlterPathToMountPath(e.FullPath);
//            var directoryName = Path.GetDirectoryName(e.FullPath);

//            var isDirectory = Directory.Exists(e.FullPath);
//            var isInSameDirectory = oldDirectoryName.Equals(directoryName);

//            DokanNet.Dokan.Notify.Rename(_dokanCurrentInstance, oldFullPath, fullPath, isDirectory, isInSameDirectory);
//        }

//        private void HookUpEvents()
//        {
//            CommonFsWatcher.Changed += OnCommonFileSystemWatcher_Changed;
//            CommonFsWatcher.Created += OnCommonFileSystemWatcher_Created;
//            CommonFsWatcher.Renamed += OnCommonFileSystemWatcher_Renamed;

//            FileFsWatcher.Deleted += OnCommonFileSystemWatcher_FileDeleted;

//            DirFsWatcher.Deleted += OnCommonFileSystemWatcher_DirectoryDeleted;
//        }

//        private void UnhookEvents()
//        {
//            CommonFsWatcher.Changed -= OnCommonFileSystemWatcher_Changed;
//            CommonFsWatcher.Created -= OnCommonFileSystemWatcher_Created;
//            CommonFsWatcher.Renamed -= OnCommonFileSystemWatcher_Renamed;

//            FileFsWatcher.Deleted -= OnCommonFileSystemWatcher_FileDeleted;

//            DirFsWatcher.Deleted -= OnCommonFileSystemWatcher_DirectoryDeleted;
//        }

//        public static DokanNotify CreateAndStart(string mirrorPath, string mountPath, DokanInstance dokanInstance)
//        {
//            var commonFsWatcher = new FileSystemWatcher(mirrorPath)
//            {
//                IncludeSubdirectories = true,
//                EnableRaisingEvents = true,
//                NotifyFilter = NotifyFilters.Attributes
//                             | NotifyFilters.CreationTime
//                             | NotifyFilters.DirectoryName
//                             | NotifyFilters.FileName
//                             | NotifyFilters.LastAccess
//                             | NotifyFilters.LastWrite
//                             | NotifyFilters.Security
//                             | NotifyFilters.Size
//            };
//            var fileFsWatcher = new FileSystemWatcher(mirrorPath)
//            {
//                IncludeSubdirectories = true,
//                EnableRaisingEvents = true,
//                NotifyFilter = NotifyFilters.FileName
//            };
//            var dirFsWatcher = new FileSystemWatcher(mirrorPath)
//            {
//                IncludeSubdirectories = true,
//                EnableRaisingEvents = true,
//                NotifyFilter = NotifyFilters.DirectoryName
//            };

//            return new DokanNotify()
//            {
//                SourcePath = mirrorPath,
//                TargetPath = mountPath,
//                DokanInstance = dokanInstance,
//                CommonFsWatcher = commonFsWatcher,
//                FileFsWatcher = fileFsWatcher,
//                DirFsWatcher = dirFsWatcher
//            };
//        }

//        public void Dispose()
//        {
//            CommonFsWatcher.Dispose();
//            FileFsWatcher.Dispose();
//            DirFsWatcher.Dispose();

//            UnhookEvents();
//        }
//    }
//}
