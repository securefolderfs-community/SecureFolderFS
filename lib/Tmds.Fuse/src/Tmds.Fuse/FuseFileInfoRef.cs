using System;
using System.Runtime.InteropServices;

namespace Tmds.Fuse
{
    public ref struct FuseFileInfoRef
    {
        Span<FuseFileInfo> _value;

        public FuseFileInfoRef(Span<FuseFileInfo> fi)
        {
            _value = fi;
        }

        public bool IsNull => _value.IsEmpty;

        public ref FuseFileInfo Value
        {
            get
            {
                if (IsNull)
                {
                    ThrowNullException();
                }

                return ref MemoryMarshal.GetReference(_value);
            }
        }

        private void ThrowNullException()
        {
            throw new NullReferenceException(typeof(FuseFileInfoRef).FullName);
        }
    }
}