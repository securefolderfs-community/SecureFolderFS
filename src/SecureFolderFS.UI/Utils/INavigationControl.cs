﻿using System;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.Utils
{
    public interface INavigationControl : IDisposable
    {
        Task<bool> NavigateAsync<TTarget, TTransition>(TTarget? target, TTransition? transition = default) where TTransition : class;
    }
}
