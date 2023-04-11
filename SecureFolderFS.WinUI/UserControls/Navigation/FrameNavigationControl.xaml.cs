using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.UI.Controls;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls.Navigation
{
    /// <summary>
    /// The base class that manages UI navigation.
    /// </summary>
    public abstract partial class FrameNavigationControl : UserControl, INavigationControl
    {
        protected FrameNavigationControl()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public abstract Task<bool> NavigateAsync<TTarget, TTransition>(TTarget target, TTransition? transition = default) where TTransition : class;

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            (ContentFrame.Content as IDisposable)?.Dispose();
        }
    }
}
