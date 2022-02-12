using System;
using System.IO;

#nullable enable

namespace SecureFolderFS.WinUI.Serialization.Implementation
{
    internal class DefaultSettingsSerializer : ISettingsSerializer
    {
        protected string? filePath;

        public virtual bool CreateFile(string path)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.Open(path, FileMode.OpenOrCreate).Dispose();
                filePath = path;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public virtual string? ReadFromFile()
        {
            ArgumentNullException.ThrowIfNull(filePath);

            return File.ReadAllText(filePath);
        }

        public virtual bool WriteToFile(string? text)
        {
            ArgumentNullException.ThrowIfNull(filePath);

            try
            {
                File.WriteAllText(filePath, text);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
