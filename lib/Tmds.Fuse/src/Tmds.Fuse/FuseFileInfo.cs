using System;

namespace Tmds.Fuse
{
    // This is named FuseFileInfo so it doesn't clash with System.IO.FileInfo
    public struct FuseFileInfo
    {
        public int flags { get; set; }
        private int _bitfields { get; set; }
        private int _padding0;
        private int _padding2;
        public ulong fh { get; set; }
        public ulong lock_owner { get; set; }
        public uint poll_events { get; set; }

        public bool writepage
        {
            get => (_bitfields & WRITEPAGE) != 0;
            set
            {
                if (value)
                    _bitfields |= WRITEPAGE;
                else
                    _bitfields &= ~WRITEPAGE;
            }
        }

        public bool direct_io
        {
            get => (_bitfields & DIRECTIO) != 0;
            set
            {
                if (value)
                    _bitfields |= DIRECTIO;
                else
                    _bitfields &= ~DIRECTIO;
            }
        }

        public bool keep_cache
        {
            get => (_bitfields & KEEPCACHE) != 0;
            set
            {
                if (value)
                    _bitfields |= KEEPCACHE;
                else
                    _bitfields &= ~KEEPCACHE;
            }
        }

        private const int WRITEPAGE = 1;
        private const int DIRECTIO = 2;
        private const int KEEPCACHE = 3;
    }
}