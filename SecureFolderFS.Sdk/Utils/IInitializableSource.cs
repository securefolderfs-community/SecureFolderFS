using System;

namespace SecureFolderFS.Sdk.Utils
{
    [Obsolete("This interface has been deprecated should no longer be used to initialize with, use IAsyncInitialize instead.")]
    internal interface IInitializableSource<TInitializeWith>
    {
        internal void Initialize(TInitializeWith param);
    }
}
