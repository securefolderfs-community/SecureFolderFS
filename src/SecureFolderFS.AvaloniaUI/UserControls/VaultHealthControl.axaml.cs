using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.AvaloniaUI.UserControls
{
    internal sealed partial class VaultHealthControl : UserControl
    {
        public VaultHealthControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public VaultHealthState VaultHealthState
        {
            get => GetValue(VaultHealthStateProperty);
            set => SetValue(VaultHealthStateProperty, value);
        }
        public static readonly StyledProperty<VaultHealthState> VaultHealthStateProperty =
            AvaloniaProperty.Register<VaultHealthControl, VaultHealthState>(nameof(VaultHealthState));

        public DateTime VaultHealthLastCheckedDate
        {
            get => GetValue(VaultHealthLastCheckedDateProperty);
            set => SetValue(VaultHealthLastCheckedDateProperty, value);
        }
        public static readonly StyledProperty<DateTime> VaultHealthLastCheckedDateProperty =
            AvaloniaProperty.Register<VaultHealthControl, DateTime>(nameof(VaultHealthLastCheckedDate), defaultValue: DateTime.UnixEpoch);

        public ICommand? StartScanningCommand
        {
            get => GetValue(StartScanningCommandProperty);
            set => SetValue(StartScanningCommandProperty, value);
        }
        public static readonly StyledProperty<ICommand?> StartScanningCommandProperty =
            AvaloniaProperty.Register<VaultHealthControl, ICommand?>(nameof(StartScanningCommand));

        public ICommand? OpenVaultHealthCommand
        {
            get => GetValue(OpenVaultHealthCommandProperty);
            set => SetValue(OpenVaultHealthCommandProperty, value);
        }
        public static readonly StyledProperty<ICommand?> OpenVaultHealthCommandProperty =
            AvaloniaProperty.Register<VaultHealthControl, ICommand?>(nameof(OpenVaultHealthCommand));
    }
}