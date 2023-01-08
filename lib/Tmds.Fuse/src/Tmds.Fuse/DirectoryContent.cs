using System;
using System.Text;

namespace Tmds.Fuse
{
    public unsafe ref struct DirectoryContent
    {
        private const int FUSE_NAME_MAX = 1024;
        private readonly void* _buffer;
        private readonly fuse_fill_dir_Delegate _fillDelegate;

        internal DirectoryContent(void* buffer, fuse_fill_dir_Delegate fillDelegate)
        {
            _buffer = buffer;
            _fillDelegate = fillDelegate;
        }

        public void AddEntry(string name) // TODO: extend API
        {
            if (name.Length > 1025) // 1025 = Encoding.UTF8.GetMaxCharCount(FUSE_NAME_MAX)
            {
                ThrowNameTooLongException();
            }
            int maxByteLength = Encoding.UTF8.GetMaxByteCount(name.Length);
            Span<byte> buffer = stackalloc byte[maxByteLength + 1]; // TODO: avoid stackalloc zero-ing
            fixed (byte* bytesPtr = buffer)
            fixed (char* charsPtr = name)
            {
                int length = Encoding.UTF8.GetBytes(charsPtr, name.Length, bytesPtr, maxByteLength);
                if (length > FUSE_NAME_MAX)
                {
                    ThrowNameTooLongException();
                }
                buffer[length] = 0;
                _fillDelegate(_buffer, bytesPtr, null, 0, 0);
            }
        }

        public void AddEntry(ReadOnlySpan<byte> name)
        {
            if (name[name.Length - 1] == 0)
            {
                if (name.Length > (FUSE_NAME_MAX + 1))
                {
                    ThrowNameTooLongException();
                }
                fixed (byte* bytesPtr = name)
                {
                    _fillDelegate(_buffer, bytesPtr, null, 0, 0);
                }
            }
            else
            {
                if (name.Length > FUSE_NAME_MAX)
                {
                    ThrowNameTooLongException();
                }
                Span<byte> buffer = stackalloc byte[name.Length + 1];
                name.CopyTo(buffer);
                buffer[name.Length] = 0;
                fixed (byte* bytesPtr = buffer)
                {
                    _fillDelegate(_buffer, bytesPtr, null, 0, 0);
                }
            }
        }

        private void ThrowNameTooLongException()
        {
            throw new ArgumentException($"The name is too long. Names may be up to {FUSE_NAME_MAX} bytes.", "name");
        }
    }
}