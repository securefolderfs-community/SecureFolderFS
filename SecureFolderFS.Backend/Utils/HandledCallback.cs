using System;

#nullable enable

namespace SecureFolderFS.Backend.Utils
{
    public sealed class HandledCallback
    {
        private readonly Action<bool> _flagCallback;

        public HandledCallback(Action<bool> flagCallback)
        {
            this._flagCallback = flagCallback;
        }

        public void Handle()
        {
            Handle(true);
        }
        
        public void Handle(bool value)
        {
            _flagCallback(value);
        }
    }
}
