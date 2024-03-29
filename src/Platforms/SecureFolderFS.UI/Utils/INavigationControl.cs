using System;
using System.Threading.Tasks;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.Utils
{
    // TODO: Needs docs
    public interface INavigationControl : IDisposable
    {
        Task<bool> NavigateAsync<TTarget, TTransition>(TTarget? target, TTransition? transition = default)
            where TTransition : class
            where TTarget : IViewDesignation;
    }
}
