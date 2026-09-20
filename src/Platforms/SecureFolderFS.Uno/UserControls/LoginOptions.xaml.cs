using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using SecureFolderFS.Core;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls
{
    public sealed partial class LoginOptions : UserControl
    {
        private bool _isLoadingOptions;

        public LoginOptions()
        {
            InitializeComponent();
        }

        private void LoginOptions_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        /// <summary>
        /// Gets or sets the vault whose per-vault file system options are shown and persisted.
        /// </summary>
        public IVaultModel? VaultModel
        {
            get => (IVaultModel?)GetValue(VaultModelProperty);
            set => SetValue(VaultModelProperty, value);
        }
        public static readonly DependencyProperty VaultModelProperty =
            DependencyProperty.Register(nameof(VaultModel), typeof(IVaultModel), typeof(LoginOptions),
                new PropertyMetadata(null, (d, _) => ((LoginOptions)d).LoadFileSystemOptions()));

        private async void LoadFileSystemOptions()
        {
            FileSystemOptionsSection.Visibility = Visibility.Collapsed;
            if (VaultModel is null)
                return;

            string effectiveFileSystemId;
            try
            {
                // Resolve the file system that would actually be used to mount this vault
                // (accounts for the preferred file system setting and device availability).
                var fileSystem = await DI.Service<IVaultFileSystemService>().GetBestFileSystemAsync();
                effectiveFileSystemId = fileSystem.Id;
            }
            catch
            {
                // No supported file system - nothing to configure
                return;
            }

            var stored = VaultModel.DataModel.FileSystemOptions;
            _isLoadingOptions = true;
            try
            {
                PortOption.Visibility = Visibility.Collapsed;
                DriveLetterOption.Visibility = Visibility.Collapsed;
                MountPathOption.Visibility = Visibility.Collapsed;

                switch (effectiveFileSystemId)
                {
                    case Constants.FileSystemId.FS_WEBDAV:
                        PortBox.Value = stored?.Port ?? 4949;
                        PortOption.Visibility = Visibility.Visible;
                        break;

#if WINDOWS
                    case Core.Dokany.Constants.FileSystem.FS_ID:
                    case WinFspId:
                        PopulateDriveLetters(stored?.MountPoint);
                        DriveLetterOption.Visibility = Visibility.Visible;
                        break;
#endif
                    
                    case Constants.FileSystemId.FS_FUSE:
#if __UNO_SKIA_MACOS__
                    case Core.MacFuse.Constants.FileSystem.FS_ID:
#endif
                        MountPathBox.Text = stored?.MountPoint ?? string.Empty;
                        MountPathOption.Visibility = Visibility.Visible;
                        break;
                }

                FileSystemOptionsSection.Visibility =
                    PortOption.Visibility == Visibility.Visible ||
                    DriveLetterOption.Visibility == Visibility.Visible ||
                    MountPathOption.Visibility == Visibility.Visible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
            }
            finally
            {
                _isLoadingOptions = false;
            }
        }

        private void PopulateDriveLetters(string? storedMountPoint)
        {
            DriveLetterBox.Items.Clear();

            var usedLetters = new HashSet<char>();
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.Name.Length > 0)
                    usedLetters.Add(char.ToUpperInvariant(drive.Name[0]));
            }

            char? storedLetter = null;
            if (!string.IsNullOrWhiteSpace(storedMountPoint))
            {
                var letter = char.ToUpperInvariant(storedMountPoint[0]);
                if (letter is >= 'A' and <= 'Z')
                    storedLetter = letter;
            }

            // Skip floppy disk drives and the system drive, and offer only free letters
            for (var letter = 'C'; letter <= 'Z'; letter++)
            {
                if (usedLetters.Contains(letter) && letter != storedLetter)
                    continue;

                DriveLetterBox.Items.Add($"{letter}:");
            }

            if (storedLetter is { } selected)
                DriveLetterBox.SelectedItem = $"{selected}:";
        }

        private void PortBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (_isLoadingOptions)
                return;

            int? port = double.IsNaN(sender.Value) ? null : (int)sender.Value;
            PersistFileSystemOptions(options => options.Port = port is > 0 and <= 65535 ? port : null);
        }

        private void DriveLetterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingOptions)
                return;

            var mountPoint = DriveLetterBox.SelectedItem as string;
            PersistFileSystemOptions(options => options.MountPoint = string.IsNullOrWhiteSpace(mountPoint) ? null : mountPoint);
        }

        private void MountPathBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_isLoadingOptions)
                return;

            var mountPoint = MountPathBox.Text;
            PersistFileSystemOptions(options => options.MountPoint = string.IsNullOrWhiteSpace(mountPoint) ? null : mountPoint.Trim());
        }

        private async void PersistFileSystemOptions(Action<FileSystemOptionsDataModel> apply)
        {
            if (VaultModel is null)
                return;

            var dataModel = VaultModel.DataModel;
            dataModel.FileSystemOptions ??= new FileSystemOptionsDataModel();
            apply(dataModel.FileSystemOptions);

            await VaultModel.TrySaveAsync();
        }

        public bool ShouldSaveCredentials
        {
            get => (bool)GetValue(ShouldSaveCredentialsProperty);
            set => SetValue(ShouldSaveCredentialsProperty, value);
        }
        public static readonly DependencyProperty ShouldSaveCredentialsProperty =
            DependencyProperty.Register(nameof(ShouldSaveCredentials), typeof(bool), typeof(LoginOptions), new PropertyMetadata(false));

        public bool AreCredentialsSaved
        {
            get => (bool)GetValue(AreCredentialsSavedProperty);
            set => SetValue(AreCredentialsSavedProperty, value);
        }
        public static readonly DependencyProperty AreCredentialsSavedProperty =
            DependencyProperty.Register(nameof(AreCredentialsSaved), typeof(bool), typeof(LoginOptions), new PropertyMetadata(false));

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(LoginOptions), new PropertyMetadata(false));

        public bool IsLoginSequence
        {
            get => (bool)GetValue(IsLoginSequenceProperty);
            set => SetValue(IsLoginSequenceProperty, value);
        }
        public static readonly DependencyProperty IsLoginSequenceProperty =
            DependencyProperty.Register(nameof(IsLoginSequence), typeof(bool), typeof(LoginOptions), new PropertyMetadata(false));

        public bool IsAlternativeLogin
        {
            get => (bool)GetValue(IsAlternativeLoginProperty);
            set => SetValue(IsAlternativeLoginProperty, value);
        }
        public static readonly DependencyProperty IsAlternativeLoginProperty =
            DependencyProperty.Register(nameof(IsAlternativeLogin), typeof(bool), typeof(LoginOptions), new PropertyMetadata(false));

        public IEnumerable? AuthenticationOptions
        {
            get => (IEnumerable?)GetValue(AuthenticationOptionsProperty);
            set => SetValue(AuthenticationOptionsProperty, value);
        }
        public static readonly DependencyProperty AuthenticationOptionsProperty =
            DependencyProperty.Register(nameof(AuthenticationOptions), typeof(IEnumerable), typeof(LoginOptions), new PropertyMetadata(null));

        public object? SelectedAuthenticationOption
        {
            get => GetValue(SelectedAuthenticationOptionProperty);
            set => SetValue(SelectedAuthenticationOptionProperty, value);
        }
        public static readonly DependencyProperty SelectedAuthenticationOptionProperty =
            DependencyProperty.Register(nameof(SelectedAuthenticationOption), typeof(object), typeof(LoginOptions), new PropertyMetadata(null));

        public ICommand? SelectAuthenticationOptionCommand
        {
            get => (ICommand?)GetValue(SelectAuthenticationOptionCommandProperty);
            set => SetValue(SelectAuthenticationOptionCommandProperty, value);
        }
        public static readonly DependencyProperty SelectAuthenticationOptionCommandProperty =
            DependencyProperty.Register(nameof(SelectAuthenticationOptionCommand), typeof(ICommand), typeof(LoginOptions), new PropertyMetadata(null));

        private void AuthenticationOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox { SelectedItem: { } selectedItem } && SelectAuthenticationOptionCommand?.CanExecute(selectedItem) == true)
                SelectAuthenticationOptionCommand.Execute(selectedItem);
        }

        public ICommand? DiscardSavedCredentialsCommand
        {
            get => (ICommand?)GetValue(DiscardSavedCredentialsCommandProperty);
            set => SetValue(DiscardSavedCredentialsCommandProperty, value);
        }
        public static readonly DependencyProperty DiscardSavedCredentialsCommandProperty =
            DependencyProperty.Register(nameof(DiscardSavedCredentialsCommand), typeof(ICommand), typeof(LoginOptions), new PropertyMetadata(null));

        public ICommand? RestartLoginCommand
        {
            get => (ICommand?)GetValue(RestartLoginCommandProperty);
            set => SetValue(RestartLoginCommandProperty, value);
        }
        public static readonly DependencyProperty RestartLoginCommandProperty =
            DependencyProperty.Register(nameof(RestartLoginCommand), typeof(ICommand), typeof(LoginOptions), new PropertyMetadata(null));

        public ICommand? RecoverAccessCommand
        {
            get => (ICommand?)GetValue(RecoverAccessCommandProperty);
            set => SetValue(RecoverAccessCommandProperty, value);
        }
        public static readonly DependencyProperty RecoverAccessCommandProperty =
            DependencyProperty.Register(nameof(RecoverAccessCommand), typeof(ICommand), typeof(LoginOptions), new PropertyMetadata(null));
    }
}
