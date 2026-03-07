using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls
{
    public sealed partial class VaultControls : UserControl
    {
        public VaultControls()
        {
            InitializeComponent();
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(VaultControls), new PropertyMetadata(false));

        public ICommand? RevealFolderCommand
        {
            get => (ICommand?)GetValue(RevealFolderCommandProperty);
            set => SetValue(RevealFolderCommandProperty, value);
        }
        public static readonly DependencyProperty RevealFolderCommandProperty =
            DependencyProperty.Register(nameof(RevealFolderCommand), typeof(ICommand), typeof(VaultControls), new PropertyMetadata(null));

        public ICommand? OpenPropertiesCommand
        {
            get => (ICommand?)GetValue(OpenPropertiesCommandProperty);
            set => SetValue(OpenPropertiesCommandProperty, value);
        }
        public static readonly DependencyProperty OpenPropertiesCommandProperty =
            DependencyProperty.Register(nameof(OpenPropertiesCommand), typeof(ICommand), typeof(VaultControls), new PropertyMetadata(null));

        public ICommand? LockVaultCommand
        {
            get => (ICommand?)GetValue(LockVaultCommandProperty);
            set => SetValue(LockVaultCommandProperty, value);
        }
        public static readonly DependencyProperty LockVaultCommandProperty =
            DependencyProperty.Register(nameof(LockVaultCommand), typeof(ICommand), typeof(VaultControls), new PropertyMetadata(null));
    }
}
