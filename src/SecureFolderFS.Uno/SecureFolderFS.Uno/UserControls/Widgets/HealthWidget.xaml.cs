using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using System;
using System.Windows.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.Widgets
{
    public sealed partial class HealthWidget : UserControl
    {
        public HealthWidget()
        {
            InitializeComponent();
        }

        public VaultHealthState VaultHealthState
        {
            get => (VaultHealthState)GetValue(VaultHealthStateProperty);
            set => SetValue(VaultHealthStateProperty, value);
        }
        public static readonly DependencyProperty VaultHealthStateProperty =
            DependencyProperty.Register(nameof(VaultHealthState), typeof(VaultHealthState), typeof(HealthWidget), new PropertyMetadata(defaultValue: null));

        public string HealthLastCheckedText
        {
            get => (string)GetValue(HealthLastCheckedTextProperty);
            set => SetValue(HealthLastCheckedTextProperty, value);
        }
        public static readonly DependencyProperty HealthLastCheckedTextProperty =
            DependencyProperty.Register(nameof(HealthLastCheckedText), typeof(string), typeof(HealthWidget), new PropertyMetadata(null));

        public ICommand? StartScanningCommand
        {
            get => (ICommand)GetValue(StartScanningCommandProperty);
            set => SetValue(StartScanningCommandProperty, value);
        }
        public static readonly DependencyProperty StartScanningCommandProperty =
            DependencyProperty.Register(nameof(StartScanningCommand), typeof(ICommand), typeof(HealthWidget), new PropertyMetadata(defaultValue: null));

        public ICommand? OpenVaultHealthCommand
        {
            get => (ICommand?)GetValue(OpenVaultHealthCommandProperty);
            set => SetValue(OpenVaultHealthCommandProperty, value);
        }
        public static readonly DependencyProperty OpenVaultHealthCommandProperty =
            DependencyProperty.Register(nameof(OpenVaultHealthCommand), typeof(ICommand), typeof(HealthWidget), new PropertyMetadata(defaultValue: null));
    }
}
