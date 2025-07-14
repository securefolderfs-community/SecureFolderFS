using Android.Content;

namespace SecureFolderFS.Maui.AppModels
{
    /// <inheritdoc cref="IDialogInterfaceOnClickListener"/>
    internal sealed class DialogOnClickListener : Java.Lang.Object, IDialogInterfaceOnClickListener
    {
        private readonly Action<IDialogInterface?, int> _onClick;

        public DialogOnClickListener(Action<IDialogInterface?, int> onClick)
        {
            _onClick = onClick;
        }

        /// <inheritdoc/>
        public void OnClick(IDialogInterface? dialog, int which)
        {
            _onClick.Invoke(dialog, which);
        }
    }
}
