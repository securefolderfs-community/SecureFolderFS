using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    /// <summary>
    /// Represents a single account row in the Accounts settings page, independent of a provider.
    /// </summary>
    [Bindable(true)]
    public sealed partial class AccountItemViewModel : PickerOptionViewModel
    {
        private readonly IAccountProvider? _provider;
        private readonly ObservableCollection<AccountItemViewModel>? _parent;

        [ObservableProperty] private string? _Subtitle;

        /// <summary>
        /// Creates a display-only item (e.g. for a picker). The remove command is a no-op.
        /// </summary>
        public AccountItemViewModel(AccountModel model)
            : base(model.Id, model.DisplayName ?? model.Id)
        {
            Subtitle = model.Subtitle;
            Icon = model.Icon;
        }

        /// <summary>
        /// Creates a manageable item backed by <paramref name="provider"/> and removable from <paramref name="parent"/>.
        /// </summary>
        public AccountItemViewModel(
            AccountModel model,
            IAccountProvider provider,
            ObservableCollection<AccountItemViewModel> parent)
            : this(model)
        {
            _provider = provider;
            _parent = parent;
        }

        [RelayCommand]
        private async Task RemoveAsync(CancellationToken cancellationToken)
        {
            if (_provider is null || _parent is null)
                return;

            await _provider.RemoveAccountAsync(Id, cancellationToken);
            _parent.Remove(this);
        }
    }
}
