﻿using System;

namespace SecureFolderFS.Sdk.Utils
{
    [Obsolete("This interface should no longer be used to initialize with, use IAsyncInitialize instead.")]
    internal interface IInitializableSource<TInitializeWith>
    {
        internal void Initialize(TInitializeWith param);
    }
}