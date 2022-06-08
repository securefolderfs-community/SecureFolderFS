using System;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls
{
    public sealed partial class VaultHealthControl : UserControl
    {
        public VaultHealthControl()
        {
            InitializeComponent();
        }


        public VaultHealthState VaultHealthState
        {
            get => (VaultHealthState)GetValue(VaultHealthStateProperty);
            set => SetValue(VaultHealthStateProperty, value);
        }
        public static readonly DependencyProperty VaultHealthStateProperty =
            DependencyProperty.Register("VaultHealthState", typeof(VaultHealthState), typeof(VaultHealthControl), new PropertyMetadata(null));


        public DateTime VaultHealthLastCheckedDate
        {
            get => (DateTime)GetValue(VaultHealthLastCheckedDateProperty);
            set => SetValue(VaultHealthLastCheckedDateProperty, value);
        }
        public static readonly DependencyProperty VaultHealthLastCheckedDateProperty =
            DependencyProperty.Register("VaultHealthLastCheckedDate", typeof(DateTime), typeof(VaultHealthControl), new PropertyMetadata(0));


        public ICommand StartScanningCommand
        {
            get => (ICommand)GetValue(StartScanningCommandProperty);
            set => SetValue(StartScanningCommandProperty, value);
        }
        public static readonly DependencyProperty StartScanningCommandProperty =
            DependencyProperty.Register("StartScanningCommand", typeof(ICommand), typeof(VaultHealthControl), new PropertyMetadata(null));


        public ICommand OpenVaultHealthCommand
        {
            get => (ICommand)GetValue(OpenVaultHealthCommandProperty);
            set => SetValue(OpenVaultHealthCommandProperty, value);
        }
        public static readonly DependencyProperty OpenVaultHealthCommandProperty =
            DependencyProperty.Register("OpenVaultHealthCommand", typeof(ICommand), typeof(VaultHealthControl), new PropertyMetadata(null));
    }
}
